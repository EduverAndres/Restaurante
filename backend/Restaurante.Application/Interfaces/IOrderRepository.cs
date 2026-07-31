using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<List<Order>> GetByRestaurantIdAsync(Guid restaurantId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
