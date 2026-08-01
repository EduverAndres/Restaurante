using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Services;

public class AIService : IAIService
{
    private const string FriendlyErrorMessage = "Lo sentimos, no pudimos procesar tu mensaje. Intenta de nuevo.";

    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly ILogger<AIService> _logger;

    public AIService(HttpClient http, IConfiguration config, ILogger<AIService> logger)
    {
        _http = http;
        _apiKey = config["DeepSeek:ApiKey"]!;
        _apiUrl = config["DeepSeek:ApiUrl"]!;
        _logger = logger;
    }

    public async Task<string> ProcessOrderConversationAsync(
        string userMessage,
        string? conversationHistory,
        string restaurantName,
        IReadOnlyList<MenuItemContext> menu,
        string? correctionInstruction = null)
    {
        var systemPrompt = BuildSystemPrompt(restaurantName, menu, correctionInstruction);

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (!string.IsNullOrEmpty(conversationHistory))
            messages.Add(new { role = "user", content = conversationHistory });

        messages.Add(new { role = "user", content = userMessage });

        var body = new { model = "deepseek-chat", messages, stream = false };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            req.Headers.Add("Authorization", $"Bearer {_apiKey}");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return content ?? "{}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "DeepSeek request failed");
            return FriendlyErrorMessage;
        }
    }

    private static string BuildSystemPrompt(string restaurantName, IReadOnlyList<MenuItemContext> menu, string? correctionInstruction)
    {
        const string schema = """{"type":"order_summary","items":[{"menuItemId":"<guid del menú>","name":"<nombre exacto del menú>","quantity":<entero positivo>,"unitPrice":<precio del menú>}],"total":<suma unitPrice*quantity>,"summary":"<resumen breve del pedido en español>","next_question":"<pregunta opcional o null>"}""";

        var menuText = menu.Count == 0
            ? "(no hay items disponibles)"
            : string.Join("\n", menu.Select(m =>
                $"- {m.CategoryName} | {m.Name} | id: {m.Id} | precio: {m.Price.ToString("0.##", CultureInfo.InvariantCulture)} | {ShortDescription(m.Description)}"));

        var prompt = string.Concat(
            "Eres un asistente amable de pedidos del restaurante ", restaurantName, ".\n",
            "Conversás en el idioma del cliente y respondés con mensajes cortos y útiles.\n\n",
            "Menú disponible (usalo SOLO para armar pedidos):\n",
            menuText, "\n\n",
            "Reglas:\n",
            "1. SOLO usás items del menú provisto. NUNCA inventes ids ni precios.\n",
            "2. Cuando el cliente pida armar o confirmar un pedido, respondés EXCLUSIVAMENTE con JSON válido con este schema exacto, sin texto adicional fuera del JSON:\n",
            schema, "\n",
            "3. menuItemId debe ser UN GUID del menú provisto; unitPrice debe ser el precio exacto del menú; total la suma exacta de unitPrice*quantity.\n",
            "4. Si el pedido aún no está claro (el cliente está preguntando o no pidió nada), respondés con texto plano conversacional SIN JSON.");

        if (!string.IsNullOrEmpty(correctionInstruction))
            prompt += "\n\n" + correctionInstruction;

        return prompt;
    }

    private static string ShortDescription(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return string.Empty;

        return description.Length <= 80 ? description : description[..80] + "...";
    }
}
