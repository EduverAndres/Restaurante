using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Reviews.Queries;

public class GetRestaurantReviewsQuery : IRequest<ApiResponse<RestaurantReviewsDto>>
{
    public Guid RestaurantId { get; set; }
}

public class GetRestaurantReviewsQueryHandler : IRequestHandler<GetRestaurantReviewsQuery, ApiResponse<RestaurantReviewsDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public GetRestaurantReviewsQueryHandler(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantReviewsDto>> Handle(GetRestaurantReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var dtos = _mapper.Map<List<ReviewDto>>(reviews);

        var count = reviews.Count;
        var average = count > 0 ? Math.Round(reviews.Average(r => r.Rating), 2) : 0;

        var response = new RestaurantReviewsDto
        {
            Reviews = dtos,
            AverageRating = average,
            Count = count
        };

        return ApiResponse<RestaurantReviewsDto>.Ok(response);
    }
}
