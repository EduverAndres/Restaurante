using AutoMapper;
using NSubstitute;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Orders.Commands;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Tests;

public class CreateOrderTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IRestaurantRepository _restaurants = Substitute.For<IRestaurantRepository>();
    private readonly IMenuItemRepository _menuItems = Substitute.For<IMenuItemRepository>();
    private readonly IAIService _ai = Substitute.For<IAIService>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IOrderNotifier _notifier = Substitute.For<IOrderNotifier>();
    private readonly CreateOrderCommandHandler _handler;

    private readonly Guid _restaurantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Restaurant _restaurant;
    private readonly MenuItem _taco;
    private readonly MenuItem _burrito;

    public CreateOrderTests()
    {
        _restaurant = new Restaurant("Test Kitchen", "test-kitchen", Guid.NewGuid())
        {
            DeliveryFee = 40,
            MinOrderAmount = 0,
        };
        _restaurant.Id = _restaurantId;
        _taco = new MenuItem("Taco", 89, _restaurantId, Guid.NewGuid());
        _burrito = new MenuItem("Burrito", 129, _restaurantId, Guid.NewGuid());

        _restaurants.GetByIdAsync(_restaurantId).Returns(_restaurant);
        _menuItems.GetByIdAsync(_taco.Id).Returns(_taco);
        _menuItems.GetByIdAsync(_burrito.Id).Returns(_burrito);
        _mapper.Map<OrderDto>(Arg.Any<object>())
            .Returns(ci => new OrderDto { Id = ((Order)ci.Args()[0]!).Id, Total = ((Order)ci.Args()[0]!).Total, Status = ((Order)ci.Args()[0]!).Status.ToString() });

        _handler = new CreateOrderCommandHandler(_orders, _restaurants, _menuItems, _ai, _mapper, _notifier);
    }

    private CreateOrderCommand Command(params CreateOrderItemDto[] items) =>
        new() { CustomerId = _customerId, RestaurantId = _restaurantId, Items = items.ToList() };

    [Fact]
    public async Task HappyPath_ComputesTotalServerSide_WithServerPrices()
    {
        Order? created = null;
        await _orders.AddAsync(Arg.Do<Order>(o => created = o));

        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = _taco.Id, Quantity = 2 },
            new CreateOrderItemDto { MenuItemId = _burrito.Id, Quantity = 1 }), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(created);
        Assert.Equal(2, created.Items.Count);
        Assert.Equal(89, created.Items[0].UnitPrice);
        Assert.Equal(129, created.Items[1].UnitPrice);
        Assert.Equal(40, created.DeliveryFee);
        Assert.Equal(89 * 2 + 129 + 40, created.Total);
        Assert.Equal(OrderStatus.Pending, created.Status);
        await _notifier.Received(1).NotifyNewOrder(_restaurantId, Arg.Any<OrderDto>());
    }

    [Fact]
    public async Task ItemFromAnotherRestaurant_IsRejected()
    {
        var foreign = new MenuItem("Foreign", 50, Guid.NewGuid(), Guid.NewGuid());
        _menuItems.GetByIdAsync(foreign.Id).Returns(foreign);

        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = foreign.Id, Quantity = 1 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Item does not belong to this restaurant", result.Message);
        await _orders.DidNotReceive().AddAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task UnavailableItem_IsRejected()
    {
        _taco.IsAvailable = false;

        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = _taco.Id, Quantity = 1 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal($"Item {_taco.Name} is not available", result.Message);
    }

    [Fact]
    public async Task UnknownItem_IsRejected()
    {
        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = Guid.NewGuid(), Quantity = 1 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.StartsWith("Menu item", result.Message);
    }

    [Fact]
    public async Task ClosedRestaurant_WithConfiguredHours_IsRejected()
    {
        // A schedule covering every day marked as closed => IsOpenNow == false.
        _restaurant.BusinessHours.AddRange(
            Enumerable.Range(0, 7).Select(day => new BusinessHour
            {
                RestaurantId = _restaurantId,
                DayOfWeek = day,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                IsClosed = true,
            }));

        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = _taco.Id, Quantity = 1 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Restaurant is closed", result.Message);
    }

    [Fact]
    public async Task BelowMinimumOrderAmount_IsRejected()
    {
        _restaurant.MinOrderAmount = 100;

        var result = await _handler.Handle(Command(
            new CreateOrderItemDto { MenuItemId = _taco.Id, Quantity = 1 }), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal($"Minimum order amount is {_restaurant.MinOrderAmount}", result.Message);
    }

    [Fact]
    public async Task UnknownRestaurant_IsRejected()
    {
        var result = await _handler.Handle(new CreateOrderCommand
        {
            CustomerId = _customerId,
            RestaurantId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>(),
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Restaurant not found", result.Message);
    }
}
