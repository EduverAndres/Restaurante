using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class UpdateRestaurantImageCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class UpdateRestaurantImageCommandHandler : IRequestHandler<UpdateRestaurantImageCommand, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public UpdateRestaurantImageCommandHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(UpdateRestaurantImageCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null || restaurant.OwnerId != request.UserId)
            throw new NotFoundException("Restaurant not found");

        if (request.Type == "logo")
            restaurant.Logo = request.Url;
        else if (request.Type == "cover")
            restaurant.CoverImage = request.Url;
        else
            throw new InvalidOperationException("Image type must be 'logo' or 'cover'");

        restaurant.UpdatedAt = DateTime.UtcNow;
        await _restaurantRepository.UpdateAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Image uploaded");
    }
}
