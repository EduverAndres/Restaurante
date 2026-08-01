using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Menu.Queries;

public class GetMenuGroupedByCategoryQuery : IRequest<ApiResponse<List<MenuGroupedByCategoryDto>>>
{
    public Guid RestaurantId { get; set; }
}

public class MenuGroupedByCategoryDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<MenuItemDto> Items { get; set; } = new();
}

public class GetMenuGroupedByCategoryQueryHandler : IRequestHandler<GetMenuGroupedByCategoryQuery, ApiResponse<List<MenuGroupedByCategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMenuItemRepository _menuItemRepository;

    public GetMenuGroupedByCategoryQueryHandler(
        ICategoryRepository categoryRepository,
        IMenuItemRepository menuItemRepository)
    {
        _categoryRepository = categoryRepository;
        _menuItemRepository = menuItemRepository;
    }

    public async Task<ApiResponse<List<MenuGroupedByCategoryDto>>> Handle(GetMenuGroupedByCategoryQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var menuItems = await _menuItemRepository.GetByRestaurantIdAsync(request.RestaurantId);

        var grouped = categories.Select(cat => new MenuGroupedByCategoryDto
        {
            Id = cat.Id,
            RestaurantId = cat.RestaurantId,
            Name = cat.Name,
            Description = cat.Description,
            DisplayOrder = cat.SortOrder,
            Items = menuItems
                .Where(m => m.CategoryId == cat.Id)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Images = m.Images,
                    IsAvailable = m.IsAvailable,
                    CategoryId = m.CategoryId,
                    CategoryName = cat.Name,
                    PreparationTime = m.PreparationTime
                }).ToList()
        }).ToList();

        return ApiResponse<List<MenuGroupedByCategoryDto>>.Ok(grouped);
    }
}