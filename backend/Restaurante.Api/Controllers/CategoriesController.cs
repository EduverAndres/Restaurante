using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Categories.Commands;
using Restaurante.Application.Features.Categories.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(Guid restaurantId)
    {
        try
        {
            var result = await _mediator.Send(new GetCategoriesByRestaurantQuery { RestaurantId = restaurantId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CategoryDto>>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(Guid restaurantId, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            var command = new CreateCategoryCommand
            {
                RestaurantId = restaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid restaurantId, Guid id, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid restaurantId, Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteCategoryCommand { Id = id });
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
    }
}