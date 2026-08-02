using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Repositories;

public class BusinessHourRepository : IBusinessHourRepository
{
    private readonly RestauranteDbContext _context;

    public BusinessHourRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task ReplaceAsync(Guid restaurantId, IEnumerable<BusinessHour> hours)
    {
        var existing = await _context.BusinessHours
            .Where(h => h.RestaurantId == restaurantId)
            .ToListAsync();

        _context.BusinessHours.RemoveRange(existing);
        await _context.BusinessHours.AddRangeAsync(hours);
        await _context.SaveChangesAsync();
    }
}
