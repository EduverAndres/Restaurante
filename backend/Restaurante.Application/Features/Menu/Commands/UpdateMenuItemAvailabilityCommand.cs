using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Exceptions;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Menu.Commands;

public class UpdateMenuItemAvailabilityCommand : IRequest<ApiResponse<MenuItemDto>>
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
    public bool IsAvailable { get; set; }
}

public class UpdateMenuItemAvailabilityCommandHandler : IRequestHandler<UpdateMenuItemAvailabilityCommand, ApiResponse<MenuItemDto>>
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public UpdateMenuItemAvailabilityCommandHandler(
        IMenuItemRepository menuItemRepository,
        IRestaurantRepository restaurantRepository,
        IMapper mapper)
    {
        _menuItemRepository = menuItemRepository;
        _restaurantRepository = restaurantRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<MenuItemDto>> Handle(UpdateMenuItemAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var item = await _menuItemRepository.GetByIdAsync(request.ItemId);
        if (item is null || item.RestaurantId != request.RestaurantId)
            throw new NotFoundException("Menu item not found");

        var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId);
        if (restaurant is null || restaurant.OwnerId != request.UserId)
            throw new NotFoundException("Restaurant not found");

        item.IsAvailable = request.IsAvailable;
        item.UpdatedAt = DateTime.UtcNow;
        await _menuItemRepository.UpdateAsync(item);

        var dto = _mapper.Map<MenuItemDto>(item);
        return ApiResponse<MenuItemDto>.Ok(dto, "Availability updated");
    }
}
