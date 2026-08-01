using System.Globalization;
using System.Text.Json;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.AI;

internal static class AIResponseValidator
{
    private const decimal CentsTolerance = 0.01m;
    private const string OrderSummaryType = "order_summary";

    public static IReadOnlyList<MenuItemContext> BuildMenuContext(IEnumerable<MenuItem> menuItems)
    {
        return menuItems
            .Where(m => m.IsAvailable)
            .Select(m => new MenuItemContext(m.Id, m.Name, m.Price, m.Description, m.Category?.Name ?? string.Empty))
            .ToList();
    }

    public static async Task<string> GetValidatedResponseAsync(
        IAIService aiService,
        string userMessage,
        string? conversationHistory,
        string restaurantName,
        IReadOnlyList<MenuItemContext> menuContext,
        IReadOnlyList<MenuItem> menuItems)
    {
        var response = await aiService.ProcessOrderConversationAsync(
            userMessage, conversationHistory, restaurantName, menuContext);

        if (!IsOrderSummary(response) || TryValidateOrderSummary(response, menuItems))
            return response;

        var retry = await aiService.ProcessOrderConversationAsync(
            userMessage, conversationHistory, restaurantName, menuContext, BuildCorrectionInstruction(menuContext));

        if (!IsOrderSummary(retry) || TryValidateOrderSummary(retry, menuItems))
            return retry;

        return BuildFallbackText(retry);
    }

    private static bool IsOrderSummary(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && TryGetProperty(doc.RootElement, "type", out var type)
                && type.ValueKind == JsonValueKind.String
                && string.Equals(type.GetString(), OrderSummaryType, StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryValidateOrderSummary(string response, IReadOnlyList<MenuItem> menuItems)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!TryGetProperty(root, "type", out var type) ||
                type.ValueKind != JsonValueKind.String ||
                !string.Equals(type.GetString(), OrderSummaryType, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!TryGetProperty(root, "items", out var items) ||
                items.ValueKind != JsonValueKind.Array ||
                items.GetArrayLength() == 0)
                return false;

            decimal expectedTotal = 0;
            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) return false;

                if (!TryGetProperty(item, "menuItemId", out var idElement) ||
                    idElement.ValueKind != JsonValueKind.String ||
                    !Guid.TryParse(idElement.GetString(), out var menuItemId))
                    return false;

                var menuItem = menuItems.FirstOrDefault(m => m.Id == menuItemId && m.IsAvailable);
                if (menuItem is null) return false;

                if (!TryGetProperty(item, "quantity", out var quantityElement) ||
                    !TryGetDecimal(quantityElement, out var quantity) ||
                    quantity <= 0 ||
                    quantity % 1 != 0)
                    return false;

                if (!TryGetProperty(item, "unitPrice", out var priceElement) ||
                    !TryGetDecimal(priceElement, out var unitPrice) ||
                    unitPrice <= 0 ||
                    Math.Abs(unitPrice - menuItem.Price) > CentsTolerance)
                    return false;

                expectedTotal += unitPrice * quantity;
            }

            if (!TryGetProperty(root, "total", out var totalElement) ||
                !TryGetDecimal(totalElement, out var total) ||
                total <= 0 ||
                Math.Abs(total - expectedTotal) > CentsTolerance)
                return false;

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string BuildCorrectionInstruction(IReadOnlyList<MenuItemContext> menuContext)
    {
        var validItems = string.Join("\n", menuContext.Select(m =>
            $"- {m.Name} (id: {m.Id}, precio: {m.Price.ToString("0.##", CultureInfo.InvariantCulture)})"));

        return "La respuesta anterior no fue válida. Usá SOLO estos ids del menú provisto:\n" +
               validItems +
               "\nRegenerá el JSON con menuItemId de esa lista, cantidades enteras positivas, " +
               "unitPrice exacto del menú y total = suma(unitPrice * quantity).";
    }

    private static string BuildFallbackText(string lastResponse)
    {
        if (TryExtractSummary(lastResponse, out var summary) && !string.IsNullOrWhiteSpace(summary))
            return summary;

        return "Lo sentimos, no pudimos armar tu pedido. Volvé a intentarlo describiendo qué querés ordenar.";
    }

    private static bool TryExtractSummary(string response, out string? summary)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            if (TryGetProperty(doc.RootElement, "summary", out var element) &&
                element.ValueKind == JsonValueKind.String)
            {
                summary = element.GetString();
                return true;
            }
        }
        catch (JsonException)
        {
        }

        summary = null;
        return false;
    }

    private static bool TryGetDecimal(JsonElement element, out decimal value)
    {
        try
        {
            value = element.GetDecimal();
            return true;
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or OverflowException)
        {
            value = 0;
            return false;
        }
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
            return true;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
