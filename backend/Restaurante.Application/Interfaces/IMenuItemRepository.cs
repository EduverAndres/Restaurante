using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IMenuItemRepository
{
    Task<List<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<MenuItem?> GetByIdAsync(Guid id);
    Task AddAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(MenuItem item);
}
