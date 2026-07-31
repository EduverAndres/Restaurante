using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Menu.Commands;

public class CreateMenuItemCommand : IRequest<ApiResponse<MenuItemDto>>
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[]? Images { get; set; }
    public Guid CategoryId { get; set; }
    public int PreparationTime { get; set; }
}

public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, ApiResponse<MenuItemDto>>
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;

    public CreateMenuItemCommandHandler(IMenuItemRepository menuItemRepository, IMapper mapper)
    {
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<MenuItemDto>> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = new MenuItem(request.Name, request.Price, request.RestaurantId, request.CategoryId)
        {
            Description = request.Description,
            Images = request.Images ?? Array.Empty<string>(),
            PreparationTime = request.PreparationTime
        };

        await _menuItemRepository.AddAsync(item);

        var dto = _mapper.Map<MenuItemDto>(item);
        return ApiResponse<MenuItemDto>.Ok(dto, "Menu item created");
    }
}
