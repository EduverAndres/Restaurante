using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Infrastructure.Data;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly RestauranteDbContext _context;

    public MenuItemRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .OrderBy(m => m.Category.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id)
    {
        return await _context.MenuItems.FindAsync(id);
    }

    public async Task<List<MenuItem>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.MenuItems
            .Where(m => m.CategoryId == categoryId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task AddAsync(MenuItem item)
    {
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem item)
    {
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(MenuItem item)
    {
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
    }
}
