using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<Category?> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}
