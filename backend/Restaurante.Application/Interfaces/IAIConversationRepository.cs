using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IAIConversationRepository
{
    Task<AIConversation?> GetByIdAsync(Guid id);
    Task<List<AIConversation>> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(AIConversation conversation);
    Task UpdateAsync(AIConversation conversation);
}
