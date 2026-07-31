using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Menu.Commands;

public class UpdateMenuItemCommand : IRequest<ApiResponse<MenuItemDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[]? Images { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
    public int PreparationTime { get; set; }
}

public class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, ApiResponse<MenuItemDto>>
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMapper _mapper;

    public UpdateMenuItemCommandHandler(IMenuItemRepository menuItemRepository, IMapper mapper)
    {
        _menuItemRepository = menuItemRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<MenuItemDto>> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _menuItemRepository.GetByIdAsync(request.Id);
        if (item is null)
            return ApiResponse<MenuItemDto>.Fail("Menu item not found");

        item.Name = request.Name;
        item.Description = request.Description;
        item.Price = request.Price;
        item.Images = request.Images ?? Array.Empty<string>();
        item.IsAvailable = request.IsAvailable;
        item.CategoryId = request.CategoryId;
        item.PreparationTime = request.PreparationTime;
        item.UpdatedAt = DateTime.UtcNow;

        await _menuItemRepository.UpdateAsync(item);

        var dto = _mapper.Map<MenuItemDto>(item);
        return ApiResponse<MenuItemDto>.Ok(dto, "Menu item updated");
    }
}
