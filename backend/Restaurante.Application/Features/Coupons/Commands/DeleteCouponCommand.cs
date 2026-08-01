using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Coupons.Commands;

public class DeleteCouponCommand : IRequest<ApiResponse<bool>>
{
    public Guid RestaurantId { get; set; }
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
}

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, ApiResponse<bool>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICouponRepository _couponRepository;

    public DeleteCouponCommandHandler(IRestaurantRepository restaurantRepository, ICouponRepository couponRepository)
    {
        _restaurantRepository = restaurantRepository;
        _couponRepository = couponRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            throw new NotFoundException("Restaurant not found");
        if (restaurant.OwnerId != request.UserId)
            return ApiResponse<bool>.Fail("Restaurant not found");

        var coupon = await _couponRepository.GetByIdAsync(request.CouponId);
        if (coupon is null || coupon.RestaurantId != request.RestaurantId)
            throw new NotFoundException("Coupon not found");

        await _couponRepository.DeleteAsync(coupon);
        return ApiResponse<bool>.Ok(true, "Coupon deleted");
    }
}
