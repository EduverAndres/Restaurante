using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "System";
    public Guid? RiderId { get; set; }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRiderRepository _riderRepository;
    private readonly IMapper _mapper;
    private readonly IOrderNotifier _notifier;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IRiderRepository riderRepository, IMapper mapper, IOrderNotifier notifier)
    {
        _orderRepository = orderRepository;
        _riderRepository = riderRepository;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ApiResponse<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            return ApiResponse<OrderDto>.Fail("Order not found");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            return ApiResponse<OrderDto>.Fail($"Invalid status: {request.Status}");

        var fromStatus = order.Status;

        if (!IsValidTransition(fromStatus, newStatus))
            return ApiResponse<OrderDto>.Fail("Invalid status transition");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatus.AssignedToRider)
        {
            if (request.RiderId.HasValue)
            {
                order.RiderId = request.RiderId;
                order.AssignedAt = DateTime.UtcNow;
            }
        }
        else if (newStatus == OrderStatus.OutForDelivery)
        {
            order.PickedUpAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Delivered)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        var history = new OrderStatusHistory(order.Id, fromStatus, newStatus, request.ChangedBy);
        order.StatusHistory.Add(history);

        await _orderRepository.UpdateAsync(order);

        if ((newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled) && order.RiderId.HasValue)
        {
            var rider = await _riderRepository.GetByIdAsync(order.RiderId.Value);
            if (rider is not null && rider.Status == RiderStatus.Busy)
            {
                rider.Status = RiderStatus.Available;
                rider.UpdatedAt = DateTime.UtcNow;
                await _riderRepository.UpdateAsync(rider);
            }
        }

        var dto = _mapper.Map<OrderDto>(order);

        // Notify restaurant of status change
        await _notifier.NotifyOrderUpdated(order.RestaurantId, dto);

        // Notify customer order group of status change
        await _notifier.NotifyOrderStatusChanged(order.Id, dto);

        return ApiResponse<OrderDto>.Ok(dto, $"Order status updated to {newStatus}");
    }

    /// <summary>
    /// Valid lifecycle transitions for an order. Delivered and Cancelled are terminal.
    /// Same-status updates are rejected (a no-op is not a transition).
    /// </summary>
    private static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return false;

        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Preparing, OrderStatus.Ready) => true,
            (OrderStatus.Preparing, OrderStatus.Cancelled) => true,
            (OrderStatus.Ready, OrderStatus.AssignedToRider) => true,
            (OrderStatus.Ready, OrderStatus.Cancelled) => true,
            (OrderStatus.AssignedToRider, OrderStatus.OutForDelivery) => true,
            (OrderStatus.AssignedToRider, OrderStatus.Cancelled) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Cancelled) => true,
            _ => false
        };
    }
}