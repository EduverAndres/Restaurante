using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IRestaurantRepository
{
    Task<List<Restaurant>> GetAllAsync();
    Task<Restaurant?> GetByIdAsync(Guid id);
    Task<Restaurant?> GetBySlugAsync(string slug);
    Task<List<Restaurant>> GetByOwnerIdAsync(Guid ownerId);
    Task AddAsync(Restaurant restaurant);
    Task UpdateAsync(Restaurant restaurant);
    Task<bool> ExistsBySlugAsync(string slug);
}
