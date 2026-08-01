using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Queries;

public class GetRestaurantsByOwnerQuery : IRequest<ApiResponse<List<RestaurantListDto>>>
{
    public Guid OwnerId { get; set; }
}

public class GetRestaurantsByOwnerQueryHandler : IRequestHandler<GetRestaurantsByOwnerQuery, ApiResponse<List<RestaurantListDto>>>
{
    private readonly IRestaurantRepository _repository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetRestaurantsByOwnerQueryHandler(IRestaurantRepository repository, IReviewRepository reviewRepository, IMapper mapper)
    {
        _repository = repository;
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RestaurantListDto>>> Handle(GetRestaurantsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _repository.GetByOwnerIdAsync(request.OwnerId);
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
