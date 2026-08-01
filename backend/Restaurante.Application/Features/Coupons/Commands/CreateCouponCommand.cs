using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.Features.Coupons.Commands;

public class CreateCouponCommand : IRequest<ApiResponse<CouponDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinOrderAmount { get; set; }
}

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, ApiResponse<CouponDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IMapper _mapper;

    public CreateCouponCommandHandler(IRestaurantRepository restaurantRepository, ICouponRepository couponRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _couponRepository = couponRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CouponDto>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null)
            throw new NotFoundException("Restaurant not found");
        if (restaurant.OwnerId != request.UserId)
            return ApiResponse<CouponDto>.Fail("Restaurant not found");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var existing = await _couponRepository.GetByCodeNormalizedAsync(normalizedCode);
        if (existing is not null)
            return ApiResponse<CouponDto>.Fail("Coupon code already exists");

        var coupon = new Coupon
        {
            Code = normalizedCode,
            DiscountType = Enum.Parse<DiscountType>(request.DiscountType, ignoreCase: true),
            DiscountValue = request.DiscountValue,
            RestaurantId = request.RestaurantId,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            MaxUses = request.MaxUses,
            MinOrderAmount = request.MinOrderAmount ?? 0,
            IsActive = true
        };

        await _couponRepository.AddAsync(coupon);

        var dto = _mapper.Map<CouponDto>(coupon);
        return ApiResponse<CouponDto>.Ok(dto, "Coupon created");
    }
}
