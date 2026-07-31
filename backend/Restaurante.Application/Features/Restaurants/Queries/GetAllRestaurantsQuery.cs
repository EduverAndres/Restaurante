using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Queries;

public class GetAllRestaurantsQuery : IRequest<ApiResponse<List<RestaurantListDto>>>
{
}

public class GetAllRestaurantsQueryHandler : IRequestHandler<GetAllRestaurantsQuery, ApiResponse<List<RestaurantListDto>>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public GetAllRestaurantsQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RestaurantListDto>>> Handle(GetAllRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _restaurantRepository.GetAllAsync();
        var dtos = _mapper.Map<List<RestaurantListDto>>(restaurants);
        return ApiResponse<List<RestaurantListDto>>.Ok(dtos);
    }
}
