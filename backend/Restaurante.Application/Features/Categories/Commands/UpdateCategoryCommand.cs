using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Categories.Commands;

public class UpdateCategoryCommand : IRequest<ApiResponse<CategoryDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
            return ApiResponse<CategoryDto>.Fail("Category not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.Icon = request.Icon;
        category.SortOrder = request.SortOrder;

        await _categoryRepository.UpdateAsync(category);

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto, "Category updated");
    }
}