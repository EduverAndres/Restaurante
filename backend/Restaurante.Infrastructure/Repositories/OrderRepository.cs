using Microsoft.EntityFrameworkCore;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;
using Restaurante.Infrastructure.Data;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RestauranteDbContext _context;

    public OrderRepository(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByRiderIdAsync(Guid riderId)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Where(o => o.RiderId == riderId)
            .ToListAsync();

        return orders
            .OrderByDescending(o => o.Status == OrderStatus.AssignedToRider || o.Status == OrderStatus.OutForDelivery)
            .ThenByDescending(o => o.CreatedAt)
            .ToList();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
