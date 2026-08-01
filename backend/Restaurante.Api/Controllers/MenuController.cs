using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Menu.Commands;
using Restaurante.Application.Features.Menu.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId}/menu")]
public class MenuController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MenuGroupedByCategoryDto>>>> GetAll(Guid restaurantId)
    {
        try
        {
            var result = await _mediator.Send(new GetMenuGroupedByCategoryQuery { RestaurantId = restaurantId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<MenuGroupedByCategoryDto>>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<List<MenuItemDto>>>> GetAllItems(Guid restaurantId)
    {
        try
        {
            var result = await _mediator.Send(new GetMenuByRestaurantQuery { RestaurantId = restaurantId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<MenuItemDto>>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> Create(Guid restaurantId, [FromBody] CreateMenuItemDto dto)
    {
        try
        {
            var command = new CreateMenuItemCommand
            {
                RestaurantId = restaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Images = dto.Images,
                CategoryId = dto.CategoryId,
                PreparationTime = dto.PreparationTime
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MenuItemDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> Update(Guid restaurantId, Guid id, [FromBody] UpdateMenuItemDto dto)
    {
        try
        {
            var command = new UpdateMenuItemCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Images = dto.Images,
                IsAvailable = dto.IsAvailable,
                CategoryId = dto.CategoryId,
                PreparationTime = dto.PreparationTime
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MenuItemDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid restaurantId, Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteMenuItemCommand { Id = id });
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
