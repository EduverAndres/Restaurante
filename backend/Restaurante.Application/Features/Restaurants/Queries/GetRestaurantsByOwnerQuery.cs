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
    private readonly IMapper _mapper;

    public GetRestaurantsByOwnerQueryHandler(IRestaurantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<RestaurantListDto>>> Handle(GetRestaurantsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _repository.GetByOwnerIdAsync(request.OwnerId);
        var dtos = _mapper.Map<List<RestaurantListDto>>(restaurants);
        return ApiResponse<List<RestaurantListDto>>.Ok(dtos);
    }
}
