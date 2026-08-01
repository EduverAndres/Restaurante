using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;
    private readonly IOrderNotifier _notifier;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        IAIService aiService,
        IMapper mapper,
        IOrderNotifier notifier)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _aiService = aiService;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ApiResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.CustomerId, request.RestaurantId);

        decimal total = 0;
        foreach (var itemDto in request.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(itemDto.MenuItemId);
            if (menuItem is null)
                return ApiResponse<OrderDto>.Fail($"Menu item {itemDto.MenuItemId} not found");

            var orderItem = new OrderItem(order.Id, itemDto.MenuItemId, itemDto.Quantity, menuItem.Price)
            {
                Notes = itemDto.Notes
            };
            order.Items.Add(orderItem);
            total += menuItem.Price * itemDto.Quantity;
        }

        order.Total = total;
        order.Notes = request.Notes;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var aiResponse = await _aiService.ProcessOrderConversationAsync(request.Notes);
            var conversation = new AIConversation(request.CustomerId, request.Notes)
            {
                OrderId = order.Id,
                Summary = aiResponse
            };
            order.AiConversation = conversation;
        }

        await _orderRepository.AddAsync(order);

        var dto = _mapper.Map<OrderDto>(order);

        // Notify restaurant of new order via SignalR
        await _notifier.NotifyNewOrder(request.RestaurantId, dto);

        return ApiResponse<OrderDto>.Ok(dto, "Order created");
    }
}