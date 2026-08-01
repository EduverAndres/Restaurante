using Restaurante.Application.DTOs;

namespace Restaurante.Application.Interfaces;

public interface IOrderNotifier
{
    Task NotifyNewOrder(Guid restaurantId, OrderDto order);
    Task NotifyOrderUpdated(Guid restaurantId, OrderDto order);
    Task NotifyOrderStatusChanged(Guid orderId, OrderDto order);
}