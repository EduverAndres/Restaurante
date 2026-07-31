using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class CreateRestaurantCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
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
        var slug = request.Name.ToLower().Replace(" ", "-");
        var restaurant = new Restaurant(request.Name, slug, request.OwnerId)
        {
            Description = request.Description,
            Logo = request.Logo,
            CoverImage = request.CoverImage
        };

        await _restaurantRepository.AddAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Restaurant created");
    }
}
