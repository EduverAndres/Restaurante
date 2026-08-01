using Microsoft.AspNetCore.SignalR;
using Restaurante.Api.Hubs;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Api.Services;

public class OrderNotifier : IOrderNotifier
{
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderNotifier(IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewOrder(Guid restaurantId, OrderDto order)
    {
        await _hubContext.Clients.Group($"restaurant_{restaurantId}").SendAsync("NewOrder", order);
    }

    public async Task NotifyOrderUpdated(Guid restaurantId, OrderDto order)
    {
        await _hubContext.Clients.Group($"restaurant_{restaurantId}").SendAsync("OrderUpdated", order);
    }

    public async Task NotifyOrderStatusChanged(Guid orderId, OrderDto order)
    {
        await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderUpdated", order);
    }

    public async Task NotifyRiderLocationUpdatedAsync(Guid orderId, double latitude, double longitude)
    {
        await _hubContext.Clients.Group($"order_{orderId}").SendAsync("RiderLocationUpdated", new { orderId, latitude, longitude });
    }
}