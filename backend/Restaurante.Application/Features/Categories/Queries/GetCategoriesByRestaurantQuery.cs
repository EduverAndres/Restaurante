using AutoMapper;
using MediatR;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Application.Features.Categories.Queries;

public class GetCategoriesByRestaurantQuery : IRequest<ApiResponse<List<CategoryDto>>>
{
    public Guid RestaurantId { get; set; }
}

public class GetCategoriesByRestaurantQueryHandler : IRequestHandler<GetCategoriesByRestaurantQuery, ApiResponse<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesByRestaurantQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategoriesByRestaurantQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetByRestaurantIdAsync(request.RestaurantId);
        var dtos = _mapper.Map<List<CategoryDto>>(categories);
        return ApiResponse<List<CategoryDto>>.Ok(dtos);
    }
}