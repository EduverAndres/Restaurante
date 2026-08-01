using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Coupons.Commands;

public class UpdateCouponCommand : IRequest<ApiResponse<CouponDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, ApiResponse<CouponDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IMapper _mapper;

    public UpdateCouponCommandHandler(IRestaurantRepository restaurantRepository, ICouponRepository couponRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CouponDto>> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            throw new NotFoundException("Restaurant not found");
        if (restaurant.OwnerId != request.UserId)
            return ApiResponse<CouponDto>.Fail("Restaurant not found");

        var coupon = await _couponRepository.GetByIdAsync(request.CouponId);
        if (coupon is null || coupon.RestaurantId != request.RestaurantId)
            throw new NotFoundException("Coupon not found");

        coupon.DiscountValue = request.DiscountValue;
        coupon.ValidFrom = request.ValidFrom;
        coupon.ValidUntil = request.ValidUntil;
        coupon.MaxUses = request.MaxUses;
        coupon.MinOrderAmount = request.MinOrderAmount ?? 0;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _couponRepository.UpdateAsync(coupon);

        var dto = _mapper.Map<CouponDto>(coupon);
        return ApiResponse<CouponDto>.Ok(dto, "Coupon updated");
    }
}
