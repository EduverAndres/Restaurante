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
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            return ApiResponse<OrderDto>.Fail("Order not found");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            return ApiResponse<OrderDto>.Fail($"Invalid status: {request.Status}");

        var fromStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        var history = new OrderStatusHistory(order.Id, fromStatus, newStatus, request.ChangedBy);
        order.StatusHistory.Add(history);

        await _orderRepository.UpdateAsync(order);

        var dto = _mapper.Map<OrderDto>(order);
        return ApiResponse<OrderDto>.Ok(dto, $"Order status updated to {newStatus}");
    }
}
