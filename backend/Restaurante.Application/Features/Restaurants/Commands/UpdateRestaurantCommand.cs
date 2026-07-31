using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class UpdateRestaurantCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
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

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.Logo = request.Logo;
        restaurant.CoverImage = request.CoverImage;
        restaurant.ThemeConfig = request.ThemeConfig;
        restaurant.IsActive = request.IsActive;
        restaurant.UpdatedAt = DateTime.UtcNow;

        await _restaurantRepository.UpdateAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Restaurant updated");
    }
}
