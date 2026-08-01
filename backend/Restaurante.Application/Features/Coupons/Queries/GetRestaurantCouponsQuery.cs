using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Coupons.Queries;

public class GetRestaurantCouponsQuery : IRequest<ApiResponse<List<CouponDto>>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
}

public class GetRestaurantCouponsQueryHandler : IRequestHandler<GetRestaurantCouponsQuery, ApiResponse<List<CouponDto>>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IMapper _mapper;

    public GetRestaurantCouponsQueryHandler(IRestaurantRepository restaurantRepository, ICouponRepository couponRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CouponDto>>> Handle(GetRestaurantCouponsQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            throw new NotFoundException("Restaurant not found");
        if (restaurant.OwnerId != request.UserId)
            return ApiResponse<List<CouponDto>>.Fail("Restaurant not found");

        var coupons = await _couponRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var dtos = _mapper.Map<List<CouponDto>>(coupons);
        return ApiResponse<List<CouponDto>>.Ok(dtos);
    }
}
