using Restaurante.Application.DTOs;

namespace Restaurante.Application.Interfaces;

public interface IAIService
{
    Task<string> ProcessOrderConversationAsync(
        string userMessage,
        string? conversationHistory,
        string restaurantName,
        IReadOnlyList<MenuItemContext> menu,
        string? correctionInstruction = null);
}
