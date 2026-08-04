using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.AI;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Helpers;

namespace Restaurante.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IAIService _aiService;
    private readonly IMapper _mapper;
    private readonly IOrderNotifier _notifier;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IRestaurantRepository restaurantRepository,
        IMenuItemRepository menuItemRepository,
        IAIService aiService,
        IMapper mapper,
        IOrderNotifier notifier)
    {
        _orderRepository = orderRepository;
        _restaurantRepository = restaurantRepository;
        _menuItemRepository = menuItemRepository;
        _aiService = aiService;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ApiResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            return ApiResponse<OrderDto>.Fail("Restaurant not found");

        // Restaurants without configured hours are never blocked.
        if (BusinessHoursHelper.IsOpenNow(restaurant.BusinessHours, DateTime.UtcNow) == false)
            return ApiResponse<OrderDto>.Fail("Restaurant is closed");

        var order = new Order(request.CustomerId, request.RestaurantId);

        decimal subtotal = 0;
        foreach (var itemDto in request.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(itemDto.MenuItemId);
            if (menuItem is null)
                return ApiResponse<OrderDto>.Fail($"Menu item {itemDto.MenuItemId} not found");

            if (menuItem.RestaurantId != request.RestaurantId)
                return ApiResponse<OrderDto>.Fail("Item does not belong to this restaurant");

            if (!menuItem.IsAvailable)
                return ApiResponse<OrderDto>.Fail($"Item {menuItem.Name} is not available");

            var orderItem = new OrderItem(order.Id, itemDto.MenuItemId, itemDto.Quantity, menuItem.Price)
            {
                Notes = itemDto.Notes
            };
            order.Items.Add(orderItem);
            subtotal += menuItem.Price * itemDto.Quantity;
        }

        if (restaurant.MinOrderAmount > 0 && subtotal < restaurant.MinOrderAmount)
            return ApiResponse<OrderDto>.Fail($"Minimum order amount is {restaurant.MinOrderAmount}");

        // Delivery fee is always server-side; the client never sends it.
        order.DeliveryFee = restaurant.DeliveryFee;
        order.Total = subtotal + restaurant.DeliveryFee;
        order.Notes = request.Notes;
        order.DeliveryAddress = request.DeliveryAddress;
        order.Latitude = request.Latitude;
        order.Longitude = request.Longitude;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var menuItems = await _menuItemRepository.GetByRestaurantIdAsync(request.RestaurantId);
            var menuContext = AIResponseValidator.BuildMenuContext(menuItems);

            var aiResponse = await _aiService.ProcessOrderConversationAsync(
                request.Notes, null, restaurant.Name, menuContext);

            var conversation = new AIConversation(request.CustomerId, "User: " + request.Notes)
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
