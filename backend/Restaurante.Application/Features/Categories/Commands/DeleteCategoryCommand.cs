using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Categories.Commands;

public class DeleteCategoryCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
            return ApiResponse<bool>.Fail("Category not found");

        await _categoryRepository.DeleteAsync(category);
        return ApiResponse<bool>.Ok(true, "Category deleted");
    }
}