using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Restaurants.Commands;
using Restaurante.Application.Features.Restaurants.Queries;
using Restaurante.Application.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<UpdateBusinessHoursDto> _businessHoursValidator;
    private readonly IValidator<UpdateDeliverySettingsDto> _deliverySettingsValidator;
    private readonly IStorageService _storage;

    public RestaurantsController(
        IMediator mediator,
        IValidator<UpdateBusinessHoursDto> businessHoursValidator,
        IValidator<UpdateDeliverySettingsDto> deliverySettingsValidator,
        IStorageService storage)
    {
        _mediator = mediator;
        _businessHoursValidator = businessHoursValidator;
        _deliverySettingsValidator = deliverySettingsValidator;
        _storage = storage;
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

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetRestaurantByIdQuery { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
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
                Slug = dto.Slug,
                Description = dto.Description,
                Logo = dto.Logo,
                CoverImage = dto.CoverImage,
                Phone = dto.Phone,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RadiusKm = dto.RadiusKm,
                DeliveryFee = dto.DeliveryFee,
                MinOrderAmount = dto.MinOrderAmount,
                EstimatedPrepTimeMinutes = dto.EstimatedPrepTimeMinutes,
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
                Slug = dto.Slug,
                Description = dto.Description,
                Logo = dto.Logo,
                CoverImage = dto.CoverImage,
                ThemeConfig = dto.ThemeConfig,
                IsActive = dto.IsActive,
                Phone = dto.Phone,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RadiusKm = dto.RadiusKm,
                DeliveryFee = dto.DeliveryFee,
                MinOrderAmount = dto.MinOrderAmount,
                EstimatedPrepTimeMinutes = dto.EstimatedPrepTimeMinutes
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RestaurantDto>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/business-hours")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateBusinessHours(Guid id, [FromBody] UpdateBusinessHoursDto dto)
    {
        var validation = await _businessHoursValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RestaurantDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateBusinessHoursCommand
        {
            RestaurantId = id,
            UserId = userId,
            Hours = dto.Hours
        });
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id:guid}/delivery-settings")]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UpdateDeliverySettings(Guid id, [FromBody] UpdateDeliverySettingsDto dto)
    {
        var validation = await _deliverySettingsValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RestaurantDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateDeliverySettingsCommand
        {
            RestaurantId = id,
            UserId = userId,
            DeliveryFee = dto.DeliveryFee,
            RadiusKm = dto.RadiusKm,
            MinOrderAmount = dto.MinOrderAmount,
            EstimatedPrepTimeMinutes = dto.EstimatedPrepTimeMinutes
        });
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("{id:guid}/dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetRestaurantDashboardQuery { RestaurantId = id, UserId = userId });
        return Ok(result);
    }

    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<RestaurantDto>>> UploadImage(Guid id, [FromForm] string type, [FromForm] IFormFile file)
    {
        if (type != "logo" && type != "cover")
            return BadRequest(ApiResponse<RestaurantDto>.Fail("Type must be 'logo' or 'cover'"));

        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<RestaurantDto>.Fail("A file is required"));

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<RestaurantDto>.Fail("Only image files are allowed"));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var url = await _storage.UploadAsync(file.OpenReadStream(), file.FileName, file.ContentType, $"restaurants/{id}");
        var result = await _mediator.Send(new UpdateRestaurantImageCommand
        {
            RestaurantId = id,
            UserId = userId,
            Type = type,
            Url = url
        });
        return Ok(result);
    }
}
