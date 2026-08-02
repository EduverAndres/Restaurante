using AutoMapper;
using NSubstitute;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Orders.Commands;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Tests;

/// <summary>
/// Exercises the real order-status state machine inside
/// UpdateOrderStatusCommandHandler (Application layer) with mocked repositories.
/// </summary>
public class OrderStatusTransitionTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IRiderRepository _riders = Substitute.For<IRiderRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IOrderNotifier _notifier = Substitute.For<IOrderNotifier>();
    private readonly UpdateOrderStatusCommandHandler _handler;

    public OrderStatusTransitionTests()
    {
        _mapper.Map<OrderDto>(Arg.Any<object>())
            .Returns(ci => new OrderDto { Id = ((Order)ci.Args()[0]!).Id, Status = ((Order)ci.Args()[0]!).Status.ToString() });
        _handler = new UpdateOrderStatusCommandHandler(_orders, _riders, _mapper, _notifier);
    }

    private static Order OrderWithStatus(OrderStatus status) =>
        new(Guid.NewGuid(), Guid.NewGuid()) { Status = status };

    private async Task<ApiResponse<OrderDto>> Update(Order order, string status, Guid? riderId = null)
    {
        _orders.GetByIdAsync(order.Id).Returns(order);
        return await _handler.Handle(new UpdateOrderStatusCommand { OrderId = order.Id, Status = status, ChangedBy = "tester", RiderId = riderId }, CancellationToken.None);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Preparing)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Ready)]
    [InlineData(OrderStatus.Preparing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Ready, OrderStatus.AssignedToRider)]
    [InlineData(OrderStatus.Ready, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.AssignedToRider, OrderStatus.OutForDelivery)]
    [InlineData(OrderStatus.AssignedToRider, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.OutForDelivery, OrderStatus.Delivered)]
    [InlineData(OrderStatus.OutForDelivery, OrderStatus.Cancelled)]
    public async Task ValidTransitions_Succeed(OrderStatus from, OrderStatus to)
    {
        var order = OrderWithStatus(from);

        var result = await Update(order, to.ToString());

        Assert.True(result.Success);
        Assert.Equal(to, order.Status);
    }

    [Fact]
    public async Task EachTransition_AppendsHistoryEntryWithFromAndTo()
    {
        var order = OrderWithStatus(OrderStatus.Pending);

        await Update(order, "Confirmed");

        var entry = Assert.Single(order.StatusHistory);
        Assert.Equal(OrderStatus.Pending, entry.FromStatus);
        Assert.Equal(OrderStatus.Confirmed, entry.ToStatus);
        Assert.Equal("tester", entry.ChangedBy);
        Assert.Equal(order.Id, entry.OrderId);
        await _orders.Received(1).UpdateAsync(order);
    }

    [Fact]
    public async Task FullLifecycle_CompletesHappyPath()
    {
        var order = OrderWithStatus(OrderStatus.Pending);

        foreach (var status in new[]
                 {
                     "Confirmed", "Preparing", "Ready",
                     "AssignedToRider", "OutForDelivery", "Delivered"
                 })
        {
            Assert.True((await Update(order, status)).Success);
        }

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.NotNull(order.DeliveredAt);
        Assert.NotNull(order.PickedUpAt);
        Assert.Equal(6, order.StatusHistory.Count);
    }

    [Fact]
    public async Task Delivered_TerminalState_RejectsAnyTransition()
    {
        var order = OrderWithStatus(OrderStatus.Delivered);

        var result = await Update(order, "Preparing");

        Assert.False(result.Success);
        Assert.Equal("Invalid status transition", result.Message);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        await _orders.DidNotReceive().UpdateAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task Cancelled_TerminalState_RejectsAnyTransition()
    {
        var order = OrderWithStatus(OrderStatus.Cancelled);

        var result = await Update(order, "Confirmed");

        Assert.False(result.Success);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task SameStatusUpdate_IsRejected()
    {
        var order = OrderWithStatus(OrderStatus.Ready);

        var result = await Update(order, "Ready");

        Assert.False(result.Success);
        Assert.Equal("Invalid status transition", result.Message);
    }

    [Fact]
    public async Task UnknownStatusString_IsRejected()
    {
        var order = OrderWithStatus(OrderStatus.Pending);

        var result = await Update(order, "Flying");

        Assert.False(result.Success);
        Assert.StartsWith("Invalid status:", result.Message);
    }

    [Fact]
    public async Task MissingOrder_ReturnsFail()
    {
        var result = await _handler.Handle(new UpdateOrderStatusCommand { OrderId = Guid.NewGuid(), Status = "Confirmed" }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Order not found", result.Message);
    }

    [Fact]
    public async Task AssignedToRider_SetsRiderAndTimestamp()
    {
        var order = OrderWithStatus(OrderStatus.Ready);
        var rider = new Rider { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };

        _riders.GetByIdAsync(rider.Id).Returns(rider);
        var result = await Update(order, "AssignedToRider", rider.Id);

        Assert.True(result.Success);
        Assert.Equal(rider.Id, order.RiderId);
        Assert.NotNull(order.AssignedAt);
    }

    [Fact]
    public async Task Delivered_ReleasesBusyRiderBackToAvailable()
    {
        var order = OrderWithStatus(OrderStatus.OutForDelivery);
        var rider = new Rider { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = RiderStatus.Busy };
        order.RiderId = rider.Id;

        _riders.GetByIdAsync(rider.Id).Returns(rider);
        Assert.True((await Update(order, "Delivered")).Success);

        Assert.Equal(RiderStatus.Available, rider.Status);
        await _riders.Received(1).UpdateAsync(rider);
    }

    [Fact]
    public async Task Delivered_DoesNotReleaseNonBusyRider()
    {
        var order = OrderWithStatus(OrderStatus.OutForDelivery);
        var rider = new Rider { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = RiderStatus.Offline };
        order.RiderId = rider.Id;

        _riders.GetByIdAsync(rider.Id).Returns(rider);
        Assert.True((await Update(order, "Delivered")).Success);

        await _riders.DidNotReceive().UpdateAsync(Arg.Any<Rider>());
    }

    [Fact]
    public async Task Transition_NotifiesRestaurantAndCustomer()
    {
        var order = OrderWithStatus(OrderStatus.Pending);

        Assert.True((await Update(order, "Confirmed")).Success);

        await _notifier.Received(1).NotifyOrderUpdated(order.RestaurantId, Arg.Any<OrderDto>());
        await _notifier.Received(1).NotifyOrderStatusChanged(order.Id, Arg.Any<OrderDto>());
    }
}
