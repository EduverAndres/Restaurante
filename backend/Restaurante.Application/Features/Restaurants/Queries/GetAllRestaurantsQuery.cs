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
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetAllRestaurantsQueryHandler(IRestaurantRepository restaurantRepository, IReviewRepository reviewRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RestaurantListDto>>> Handle(GetAllRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _restaurantRepository.GetAllAsync();
        var dtos = _mapper.Map<List<RestaurantListDto>>(restaurants);

        var summary = await _reviewRepository.GetRatingSummaryAsync(dtos.Select(d => d.Id).ToList());
        foreach (var dto in dtos)
        {
            if (summary.TryGetValue(dto.Id, out var s))
            {
                dto.AverageRating = s.AverageRating;
                dto.ReviewCount = s.ReviewCount;
            }
        }

        return ApiResponse<List<RestaurantListDto>>.Ok(dtos);
    }
}
