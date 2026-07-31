using Microsoft.EntityFrameworkCore;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;

namespace Restaurante.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly RestauranteDbContext _context;

    public CategoryRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.Categories
            .Where(c => c.RestaurantId == restaurantId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
