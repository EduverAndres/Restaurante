using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Queries;

public class GetRestaurantBySlugQuery : IRequest<ApiResponse<RestaurantDto>>
{
    public string Slug { get; set; } = string.Empty;
}

public class GetRestaurantBySlugQueryHandler : IRequestHandler<GetRestaurantBySlugQuery, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public GetRestaurantBySlugQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(GetRestaurantBySlugQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetBySlugAsync(request.Slug);
        if (restaurant is null)
            return ApiResponse<RestaurantDto>.Fail("Restaurant not found");

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto);
    }
}
