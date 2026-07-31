using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Repositories;

public class AIConversationRepository : IAIConversationRepository
{
    private readonly RestauranteDbContext _context;

    public AIConversationRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AIConversation conversation)
    {
        await _context.AIConversations.AddAsync(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task<AIConversation?> GetByIdAsync(Guid id)
    {
        return await _context.AIConversations.FindAsync(id);
    }

    public async Task UpdateAsync(AIConversation conversation)
    {
        _context.AIConversations.Update(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AIConversation>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.AIConversations
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<AIConversation?> GetByOrderAsync(Guid orderId)
    {
        return await _context.AIConversations
            .FirstOrDefaultAsync(a => a.OrderId == orderId);
    }
}
