using Microsoft.EntityFrameworkCore;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;

namespace Restaurante.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly RestauranteDbContext _context;

    public ReviewRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == orderId);
    }

    public async Task AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<Guid, RatingSummary>> GetRatingSummaryAsync(IReadOnlyCollection<Guid> restaurantIds)
    {
        if (restaurantIds.Count == 0)
            return new Dictionary<Guid, RatingSummary>();

        var rows = await _context.Reviews
            .Where(r => restaurantIds.Contains(r.RestaurantId))
            .GroupBy(r => r.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, Count = g.Count(), Avg = g.Average(r => r.Rating) })
            .ToListAsync();

        return rows.ToDictionary(
            x => x.RestaurantId,
            x => new RatingSummary(x.Count, Math.Round(x.Avg, 2)));
    }
}
