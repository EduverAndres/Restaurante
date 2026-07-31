using Microsoft.AspNetCore.SignalR;

namespace Restaurante.Api.Hubs;

public class OrderHub : Hub
{
    public async Task JoinRestaurantGroup(string restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
    }

    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task LeaveRestaurantGroup(string restaurantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }
}
