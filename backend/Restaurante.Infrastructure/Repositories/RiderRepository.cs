using Microsoft.EntityFrameworkCore;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;
using Restaurante.Infrastructure.Data;

namespace Restaurante.Infrastructure.Repositories;

public class RiderRepository : IRiderRepository
{
    private readonly RestauranteDbContext _context;

    public RiderRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<Rider?> GetByIdAsync(Guid id)
    {
        return await _context.Riders.FindAsync(id);
    }

    public async Task<Rider?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Riders.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<List<Rider>> GetAvailableAsync()
    {
        return await _context.Riders
            .Where(r => r.Status == RiderStatus.Available)
            .ToListAsync();
    }

    public async Task AddAsync(Rider rider)
    {
        await _context.Riders.AddAsync(rider);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Rider rider)
    {
        _context.Riders.Update(rider);
        await _context.SaveChangesAsync();
    }
}
