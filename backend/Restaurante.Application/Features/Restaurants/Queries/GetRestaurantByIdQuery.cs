using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Queries;

public class GetRestaurantByIdQuery : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid Id { get; set; }
}

public class GetRestaurantByIdQueryHandler : IRequestHandler<GetRestaurantByIdQuery, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public GetRestaurantByIdQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.Id);
        if (restaurant == null)
            return ApiResponse<RestaurantDto>.Fail("Restaurant not found");

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto);
    }
}