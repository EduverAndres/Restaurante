using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Features.Categories.Commands;

public class CreateCategoryCommand : IRequest<ApiResponse<CategoryDto>>
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.RestaurantId)
        {
            Description = request.Description,
            Icon = request.Icon,
            SortOrder = request.SortOrder
        };

        await _categoryRepository.AddAsync(category);

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto, "Category created");
    }
}