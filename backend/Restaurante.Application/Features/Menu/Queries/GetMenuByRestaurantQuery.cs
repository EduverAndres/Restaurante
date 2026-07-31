using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Menu.Queries;

public class GetMenuByRestaurantQuery : IRequest<ApiResponse<List<MenuItemDto>>>
{
    public Guid RestaurantId { get; set; }
}

public class GetMenuByRestaurantQueryHandler : IRequestHandler<GetMenuByRestaurantQuery, ApiResponse<List<MenuItemDto>>>
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;

    public GetMenuByRestaurantQueryHandler(IMenuItemRepository menuItemRepository, IMapper mapper)
    {
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<MenuItemDto>>> Handle(GetMenuByRestaurantQuery request, CancellationToken cancellationToken)
    {
        var items = await _menuItemRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var dtos = _mapper.Map<List<MenuItemDto>>(items);
        return ApiResponse<List<MenuItemDto>>.Ok(dtos);
    }
}
