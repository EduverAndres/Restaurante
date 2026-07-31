using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public AIService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["DeepSeek:ApiKey"]!;
        _apiUrl = config["DeepSeek:ApiUrl"]!;
    }

    public async Task<string> ProcessOrderConversationAsync(string userMessage, string? conversationHistory = null)
    {
        var systemPrompt = @"Eres un asistente de pedidos para un restaurante. 
Tu función es ayudar al cliente a armar su pedido de forma conversacional.
Debes:
1. Preguntar qué platillo desea (basado en el menú disponible)
2. Preguntar cantidad y modificaciones
3. Confirmar el pedido completo
4. Responder SOLO con JSON válido con esta estructura:
{
  ""items"": [{""name"": ""..."", ""quantity"": 1, ""notes"": ""...""}],
  ""total"": 0.00,
  ""summary"": ""Resumen del pedido"",
  ""next_question"": ""Próxima pregunta al cliente""
}
Sé amable y profesional.";

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (!string.IsNullOrEmpty(conversationHistory))
        {
            var history = JsonSerializer.Deserialize<List<object>>(conversationHistory);
            if (history != null) messages.AddRange(history);
        }

        messages.Add(new { role = "user", content = userMessage });

        var body = new { model = "deepseek-chat", messages, stream = false };

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
}
