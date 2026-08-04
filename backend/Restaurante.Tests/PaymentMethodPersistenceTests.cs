using AutoMapper;
using NSubstitute;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Orders.Commands;
using Restaurante.Application.Features.Payments.Commands;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Tests;

/// <summary>
/// Proves PaymentMethod, DeliveryAddress, Latitude and Longitude are
/// persisted on the Order through the Application-layer handlers
/// (mocked repositories, no live database).
/// </summary>
public class PaymentMethodPersistenceTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IRestaurantRepository _restaurants = Substitute.For<IRestaurantRepository>();
    private readonly IMenuItemRepository _menuItems = Substitute.For<IMenuItemRepository>();
    private readonly IAIService _ai = Substitute.For<IAIService>();
    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IPaymentService _paymentService = Substitute.For<IPaymentService>();
    private readonly IOrderNotifier _notifier = Substitute.For<IOrderNotifier>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private readonly Guid _restaurantId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Restaurant _restaurant;
    private readonly MenuItem _taco;

    public PaymentMethodPersistenceTests()
    {
        _restaurant = new Restaurant("Test Kitchen", "test-kitchen", Guid.NewGuid())
        {
            DeliveryFee = 40,
            MinOrderAmount = 0,
        };
        _restaurant.Id = _restaurantId;
        _taco = new MenuItem("Taco", 89, _restaurantId, Guid.NewGuid());

        _restaurants.GetByIdAsync(_restaurantId).Returns(_restaurant);
        _menuItems.GetByIdAsync(_taco.Id).Returns(_taco);
    }

    [Fact]
    public async Task CreateOrder_PersistsDeliveryAddressAndCoordinates()
    {
        Order? created = null;
        await _orders.AddAsync(Arg.Do<Order>(o => created = o));

        var handler = new CreateOrderCommandHandler(
            _orders, _restaurants, _menuItems, _ai, _mapper, _notifier);

        var result = await handler.Handle(new CreateOrderCommand
        {
            CustomerId = _customerId,
            RestaurantId = _restaurantId,
            Items = new List<CreateOrderItemDto> { new() { MenuItemId = _taco.Id, Quantity = 1 } },
            DeliveryAddress = "Calle 93 # 13-42, Bogotá",
            Latitude = 4.6789,
            Longitude = -74.0489,
        }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(created);
        Assert.Equal("Calle 93 # 13-42, Bogotá", created.DeliveryAddress);
        Assert.Equal(4.6789, created.Latitude);
        Assert.Equal(-74.0489, created.Longitude);
    }

    [Fact]
    public async Task ProcessPaidCashPayment_SetsOrderPaymentMethodToCash()
    {
        var order = new Order(_customerId, _restaurantId)
        {
            Total = 129,
            PaymentStatus = PaymentStatus.Pending,
        };
        _orders.GetByIdAsync(order.Id).Returns(order);
        _payments.GetByOrderIdAsync(order.Id).Returns(new List<Payment>());
        _paymentService.ProcessPaymentAsync(order, Arg.Any<ProcessPaymentDto>())
            .Returns(new PaymentResult(true, "CASH-abc", null, "Paid", "Payment collected on delivery", null));
        _mapper.Map<PaymentDto>(Arg.Any<object>())
            .Returns(ci => new PaymentDto
            {
                Id = ((Payment)ci.Args()[0]!).Id,
                Method = ((Payment)ci.Args()[0]!).Method,
                Status = ((Payment)ci.Args()[0]!).Status.ToString(),
            });

        var handler = new ProcessPaymentCommandHandler(
            _orders, _payments, _paymentService, _notifier, _mapper);

        var result = await handler.Handle(new ProcessPaymentCommand
        {
            OrderId = order.Id,
            CustomerId = _customerId,
            Method = "CASH",
        }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Paid", result.Data.Status);
        Assert.Equal("CASH", result.Data.Method);
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal("CASH", order.PaymentMethod);
        await _orders.Received(1).UpdateAsync(order);
    }

    [Fact]
    public async Task WebhookApprovedCardPayment_SetsOrderPaymentMethodWhenMissing()
    {
        var payment = new Payment(Guid.NewGuid(), 129, "CARD");
        var order = new Order(Guid.NewGuid(), _restaurantId)
        {
            PaymentStatus = PaymentStatus.Pending,
        };
        payment.OrderId = order.Id;

        _paymentService.VerifyWebhookSignatureAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _payments.GetByTransactionIdAsync("txn_card_1").Returns(payment);
        _orders.GetByIdAsync(order.Id).Returns(order);

        var handler = new PaymentWebhookCommandHandler(
            _paymentService, _payments, _orders, _notifier, _mapper);

        var result = await handler.Handle(new PaymentWebhookCommand
        {
            RawBody = "{\"event\":\"transaction.updated\"}",
            Signature = "valid",
            Payload = new PaymentWebhookDto
            {
                Event = "transaction.updated",
                Data = new PaymentWebhookDataDto
                {
                    Id = "txn_card_1",
                    Status = "APPROVED",
                    Reference = "rest-ref",
                },
            },
        }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal("CARD", order.PaymentMethod);
        await _orders.Received(1).UpdateAsync(order);
    }
}