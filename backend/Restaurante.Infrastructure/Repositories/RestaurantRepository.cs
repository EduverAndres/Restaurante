using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly RestauranteDbContext _context;

    public RestaurantRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<Restaurant?> GetByIdAsync(Guid id)
    {
        return await _context.Restaurants.FindAsync(id);
    }

    public async Task<Restaurant?> GetBySlugAsync(string slug)
    {
        return await _context.Restaurants.FirstOrDefaultAsync(r => r.Slug == slug);
    }

    public async Task<List<Restaurant>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Restaurants
            .Where(r => r.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Restaurant>> GetAllAsync()
    {
        return await _context.Restaurants.ToListAsync();
    }

    public async Task AddAsync(Restaurant restaurant)
    {
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Restaurant restaurant)
    {
        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Restaurant restaurant)
    {
        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();
    }
}
