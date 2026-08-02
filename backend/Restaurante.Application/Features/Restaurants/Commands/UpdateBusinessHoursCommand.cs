using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Restaurants.Commands;

public class UpdateBusinessHoursCommand : IRequest<ApiResponse<RestaurantDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public List<BusinessHourDto> Hours { get; set; } = new();
}

public class UpdateBusinessHoursCommandHandler : IRequestHandler<UpdateBusinessHoursCommand, ApiResponse<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IBusinessHourRepository _businessHourRepository;
    private readonly IMapper _mapper;

    public UpdateBusinessHoursCommandHandler(
        IRestaurantRepository restaurantRepository,
        IBusinessHourRepository businessHourRepository,
        IMapper mapper)
    {
        _restaurantRepository = restaurantRepository;
        _businessHourRepository = businessHourRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RestaurantDto>> Handle(UpdateBusinessHoursCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null || restaurant.OwnerId != request.UserId)
            throw new NotFoundException("Restaurant not found");

        var hours = request.Hours.Select(h => new BusinessHour
        {
            RestaurantId = request.RestaurantId,
            DayOfWeek = h.DayOfWeek,
            OpenTime = h.OpenTime,
            CloseTime = h.CloseTime,
            IsClosed = h.IsClosed
        }).ToList();

        await _businessHourRepository.ReplaceAsync(request.RestaurantId, hours);

        restaurant.BusinessHours = hours;
        var dto = _mapper.Map<RestaurantDto>(restaurant);
        return ApiResponse<RestaurantDto>.Ok(dto, "Business hours updated");
    }
}
