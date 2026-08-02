using AutoMapper;
using NSubstitute;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Features.Orders.Commands;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Tests;

public class ApplyCouponTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly ICouponRepository _coupons = Substitute.For<ICouponRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly ApplyCouponCommandHandler _handler;

    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Order _order;
    private readonly Coupon _coupon;

    public ApplyCouponTests()
    {
        _order = new Order(_customerId, Guid.NewGuid()) { Total = 200, DeliveryFee = 40 };
        _coupon = new Coupon
        {
            Code = "WELCOME10",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            ValidFrom = DateTime.UtcNow.AddDays(-10),
            ValidUntil = DateTime.UtcNow.AddDays(10),
            MaxUses = 100,
            IsActive = true,
        };

        _orders.GetByIdAsync(_order.Id).Returns(_order);
        _coupons.GetByCodeAsync(_coupon.Code).Returns(_coupon);
        _mapper.Map<OrderDto>(Arg.Any<object>())
            .Returns(ci => new OrderDto { Id = ((Order)ci.Args()[0]!).Id, Total = ((Order)ci.Args()[0]!).Total });

        _handler = new ApplyCouponCommandHandler(_orders, _coupons, _mapper);
    }

    private Task<ApiResponse<OrderDto>> Apply() =>
        _handler.Handle(new ApplyCouponCommand { OrderId = _order.Id, UserId = _customerId, Code = _coupon.Code }, CancellationToken.None);

    [Fact]
    public async Task PercentageCoupon_AppliesDiscount_DecrementsTotal_AndIncrementsTimesUsed()
    {
        var result = await Apply();

        Assert.True(result.Success);
        Assert.Equal(20, _order.DiscountAmount);   // 10% of 200
        Assert.Equal(180, _order.Total);
        Assert.Equal(_coupon.Id, _order.CouponId);
        Assert.Equal(1, _coupon.TimesUsed);
        await _orders.Received(1).UpdateAsync(_order);
        await _coupons.Received(1).UpdateAsync(_coupon);
    }

    [Fact]
    public async Task PercentageDiscount_IsCappedAtOrderTotal()
    {
        _coupon.DiscountValue = 150; // 150% of 200 => capped at 200
        var result = await Apply();

        Assert.True(result.Success);
        Assert.Equal(200, _order.DiscountAmount);
        Assert.Equal(0, _order.Total);
    }

    [Fact]
    public async Task FixedCoupon_SubtractsExactValue()
    {
        _coupon.DiscountType = DiscountType.Fixed;
        _coupon.DiscountValue = 25;

        var result = await Apply();

        Assert.True(result.Success);
        Assert.Equal(25, _order.DiscountAmount);
        Assert.Equal(175, _order.Total);
    }

    [Fact]
    public async Task FixedDiscount_IsCappedAtOrderTotal()
    {
        _coupon.DiscountType = DiscountType.Fixed;
        _coupon.DiscountValue = 500;

        var result = await Apply();

        Assert.True(result.Success);
        Assert.Equal(200, _order.DiscountAmount);
        Assert.Equal(0, _order.Total);
    }

    [Fact]
    public async Task ExpiredCoupon_IsRejected()
    {
        _coupon.ValidUntil = DateTime.UtcNow.AddHours(-1);

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Coupon has expired", result.Message);
        Assert.Equal(0, _coupon.TimesUsed);
    }

    [Fact]
    public async Task InactiveCoupon_IsRejected()
    {
        _coupon.IsActive = false;

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Coupon is not active", result.Message);
    }

    [Fact]
    public async Task CouponNotYetValid_IsRejected()
    {
        _coupon.ValidFrom = DateTime.UtcNow.AddDays(1);

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Coupon is not valid yet", result.Message);
    }

    [Fact]
    public async Task MaxUsesReached_IsRejected()
    {
        _coupon.TimesUsed = _coupon.MaxUses!.Value;

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Coupon usage limit reached", result.Message);
    }

    [Fact]
    public async Task MinOrderAmountNotMet_IsRejected()
    {
        _coupon.MinOrderAmount = 250;

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal($"Minimum order amount for this coupon is {_coupon.MinOrderAmount}", result.Message);
    }

    [Fact]
    public async Task CouponForAnotherRestaurant_IsRejected()
    {
        _coupon.RestaurantId = Guid.NewGuid();

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Coupon is not valid for this restaurant", result.Message);
    }

    [Fact]
    public async Task OrderNotPending_IsRejected()
    {
        _order.Status = OrderStatus.Confirmed;

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Order is not pending", result.Message);
    }

    [Fact]
    public async Task OrderWithCouponAlreadyApplied_IsRejected()
    {
        _order.CouponId = Guid.NewGuid();

        var result = await Apply();

        Assert.False(result.Success);
        Assert.Equal("Order already has a coupon applied", result.Message);
    }

    [Fact]
    public async Task OrderOfAnotherUser_IsRejected()
    {
        var result = await _handler.Handle(new ApplyCouponCommand
        {
            OrderId = _order.Id,
            UserId = Guid.NewGuid(),
            Code = _coupon.Code,
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Order does not belong to this user", result.Message);
    }

    [Fact]
    public async Task UnknownCoupon_IsRejected()
    {
        var result = await _handler.Handle(new ApplyCouponCommand
        {
            OrderId = _order.Id,
            UserId = _customerId,
            Code = "NOPE",
        }, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Coupon not found", result.Message);
    }

    [Fact]
    public async Task UnknownOrder_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(new ApplyCouponCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = _customerId,
            Code = _coupon.Code,
        }, CancellationToken.None));
    }
}
