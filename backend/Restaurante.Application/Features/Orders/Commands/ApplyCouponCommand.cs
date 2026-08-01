using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Orders.Commands;

public class ApplyCouponCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IMapper _mapper;

    public ApplyCouponCommandHandler(IOrderRepository orderRepository, ICouponRepository couponRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
            throw new NotFoundException("Order not found");

        if (order.CustomerId != request.UserId)
            return ApiResponse<OrderDto>.Fail("Order does not belong to this user");

        if (order.Status != OrderStatus.Pending)
            return ApiResponse<OrderDto>.Fail("Order is not pending");

        if (order.CouponId.HasValue)
            return ApiResponse<OrderDto>.Fail("Order already has a coupon applied");

        var coupon = await _couponRepository.GetByCodeAsync(request.Code);
        if (coupon is null)
            return ApiResponse<OrderDto>.Fail("Coupon not found");

        if (!coupon.IsActive)
            return ApiResponse<OrderDto>.Fail("Coupon is not active");

        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom)
            return ApiResponse<OrderDto>.Fail("Coupon is not valid yet");
        if (now > coupon.ValidUntil)
            return ApiResponse<OrderDto>.Fail("Coupon has expired");

        if (coupon.MaxUses.HasValue && coupon.TimesUsed >= coupon.MaxUses.Value)
            return ApiResponse<OrderDto>.Fail("Coupon usage limit reached");

        if (coupon.RestaurantId.HasValue && coupon.RestaurantId != order.RestaurantId)
            return ApiResponse<OrderDto>.Fail("Coupon is not valid for this restaurant");

        if (coupon.MinOrderAmount > 0 && order.Total < coupon.MinOrderAmount)
            return ApiResponse<OrderDto>.Fail($"Minimum order amount for this coupon is {coupon.MinOrderAmount}");

        var discount = coupon.DiscountType == DiscountType.Percentage
            ? Math.Min(Math.Round(order.Total * coupon.DiscountValue / 100, 2), order.Total)
            : Math.Min(coupon.DiscountValue, order.Total);

        order.CouponId = coupon.Id;
        order.DiscountAmount = discount;
        order.Total -= discount;
        order.UpdatedAt = DateTime.UtcNow;
        coupon.TimesUsed++;

        await _orderRepository.UpdateAsync(order);
        await _couponRepository.UpdateAsync(coupon);

        return ApiResponse<OrderDto>.Ok(_mapper.Map<OrderDto>(order), "Coupon applied");
    }
}
