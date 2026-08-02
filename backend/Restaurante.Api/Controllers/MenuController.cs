using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Menu.Commands;
using Restaurante.Application.Features.Menu.Queries;
using Restaurante.Application.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId}/menu")]
public class MenuController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStorageService _storage;

    public MenuController(IMediator mediator, IStorageService storage)
    {
        _mediator = mediator;
        _storage = storage;
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

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPatch("{itemId:guid}/availability")]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> SetAvailability(Guid restaurantId, Guid itemId, [FromBody] UpdateMenuItemAvailabilityDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateMenuItemAvailabilityCommand
        {
            RestaurantId = restaurantId,
            UserId = userId,
            ItemId = itemId,
            IsAvailable = dto.IsAvailable
        });
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{itemId:guid}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MenuItemDto>>> UploadImage(Guid restaurantId, Guid itemId, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<MenuItemDto>.Fail("A file is required"));

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<MenuItemDto>.Fail("Only image files are allowed"));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var url = await _storage.UploadAsync(file.OpenReadStream(), file.FileName, file.ContentType, $"menu/{restaurantId}");
        var result = await _mediator.Send(new UpdateMenuItemImageCommand
        {
            RestaurantId = restaurantId,
            UserId = userId,
            ItemId = itemId,
            Url = url
        });
        return Ok(result);
    }
}
