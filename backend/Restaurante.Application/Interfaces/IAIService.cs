namespace Restaurante.Application.Interfaces;

public interface IAIService
{
    Task<string> ProcessOrderConversationAsync(string userMessage, string? conversationHistory = null);
}
