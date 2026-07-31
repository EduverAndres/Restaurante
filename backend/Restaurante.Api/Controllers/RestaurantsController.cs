using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Restaurants.Commands;
using Restaurante.Application.Features.Restaurants.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RestaurantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RestaurantListDto>>>> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetAllRestaurantsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RestaurantListDto>>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("owner")]
    public async Task<ActionResult<ApiResponse<List<RestaurantListDto>>>> GetByOwner()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetRestaurantsByOwnerQuery { OwnerId = userId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<RestaurantListDto>>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetBySlug(string slug)
    {
        try
        {
            var result = await _mediator.Send(new GetRestaurantBySlugQuery { Slug = slug });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> Create([FromBody] CreateRestaurantDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateRestaurantCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo,
                CoverImage = dto.CoverImage,
                OwnerId = userId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> Update(Guid id, [FromBody] UpdateRestaurantDto dto)
    {
        try
        {
            var command = new UpdateRestaurantCommand
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo,
                CoverImage = dto.CoverImage,
                ThemeConfig = dto.ThemeConfig,
                IsActive = dto.IsActive
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.Fail(ex.Message));
        }
    }
}
