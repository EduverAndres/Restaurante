using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class UpdateRestaurantCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal? DeliveryFee { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
}

public class UpdateRestaurantCommandHandler : IRequestHandler<UpdateRestaurantCommand, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public UpdateRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.Id);
        if (restaurant is null)
            return ApiResponse<RestaurantDto>.Fail("Restaurant not found");

        if (!string.IsNullOrWhiteSpace(request.Slug) &&
            !string.Equals(request.Slug, restaurant.Slug, StringComparison.OrdinalIgnoreCase))
        {
            if (await _restaurantRepository.ExistsBySlugAsync(request.Slug))
                return ApiResponse<RestaurantDto>.Fail("Slug is already in use");
            restaurant.Slug = request.Slug.ToLower();
        }

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.Logo = request.Logo;
        restaurant.CoverImage = request.CoverImage;
        restaurant.ThemeConfig = request.ThemeConfig;
        restaurant.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Phone))
            restaurant.Phone = request.Phone;
        if (request.Latitude.HasValue)
            restaurant.Latitude = request.Latitude;
        if (request.Longitude.HasValue)
            restaurant.Longitude = request.Longitude;
        if (request.RadiusKm.HasValue)
            restaurant.RadiusKm = request.RadiusKm;
        if (request.DeliveryFee.HasValue)
            restaurant.DeliveryFee = request.DeliveryFee.Value;
        if (request.MinOrderAmount.HasValue)
            restaurant.MinOrderAmount = request.MinOrderAmount.Value;
        if (request.EstimatedPrepTimeMinutes.HasValue)
            restaurant.EstimatedPrepTimeMinutes = request.EstimatedPrepTimeMinutes;
        restaurant.UpdatedAt = DateTime.UtcNow;

        await _restaurantRepository.UpdateAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Restaurant updated");
    }
}
