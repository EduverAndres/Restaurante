using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class CreateRestaurantCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
    public Guid OwnerId { get; set; }
}

public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public CreateRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? request.Name.ToLower().Replace(" ", "-")
            : request.Slug.ToLower();

        if (await _restaurantRepository.ExistsBySlugAsync(slug))
            return ApiResponse<RestaurantDto>.Fail("Slug is already in use");

        var restaurant = new Restaurant(request.Name, slug, request.OwnerId)
        {
            Description = request.Description,
            Logo = request.Logo,
            CoverImage = request.CoverImage,
            Phone = request.Phone,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusKm = request.RadiusKm,
            DeliveryFee = request.DeliveryFee,
            MinOrderAmount = request.MinOrderAmount,
            EstimatedPrepTimeMinutes = request.EstimatedPrepTimeMinutes
        };

        await _restaurantRepository.AddAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Restaurant created");
    }
}
