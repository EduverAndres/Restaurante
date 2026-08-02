using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class UpdateDeliverySettingsCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public decimal DeliveryFee { get; set; }
    public double? RadiusKm { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
}

public class UpdateDeliverySettingsCommandHandler : IRequestHandler<UpdateDeliverySettingsCommand, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public UpdateDeliverySettingsCommandHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(UpdateDeliverySettingsCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null || restaurant.OwnerId != request.UserId)
            throw new NotFoundException("Restaurant not found");

        restaurant.DeliveryFee = request.DeliveryFee;
        restaurant.RadiusKm = request.RadiusKm;
        restaurant.MinOrderAmount = request.MinOrderAmount;
        restaurant.EstimatedPrepTimeMinutes = request.EstimatedPrepTimeMinutes;
        restaurant.UpdatedAt = DateTime.UtcNow;

        await _restaurantRepository.UpdateAsync(restaurant);

        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Delivery settings updated");
    }
}
