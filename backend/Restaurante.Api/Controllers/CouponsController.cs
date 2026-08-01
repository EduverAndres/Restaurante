using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Coupons.Commands;
using Restaurante.Application.Features.Coupons.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/coupons")]
[Authorize(Roles = "RestaurantOwner")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateCouponDto> _createValidator;
    private readonly IValidator<UpdateCouponDto> _updateValidator;

    public CouponsController(IMediator mediator, IValidator<CreateCouponDto> createValidator, IValidator<UpdateCouponDto> updateValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CouponDto>>>> GetAll(Guid restaurantId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetRestaurantCouponsQuery { RestaurantId = restaurantId, UserId = userId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CouponDto>>> Create(Guid restaurantId, [FromBody] CreateCouponDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CouponDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateCouponCommand
        {
            RestaurantId = restaurantId,
            UserId = userId,
            Code = dto.Code,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            MaxUses = dto.MaxUses,
            MinOrderAmount = dto.MinOrderAmount
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{couponId:guid}")]
    public async Task<ActionResult<ApiResponse<CouponDto>>> Update(Guid restaurantId, Guid couponId, [FromBody] UpdateCouponDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CouponDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UpdateCouponCommand
        {
            RestaurantId = restaurantId,
            CouponId = couponId,
            UserId = userId,
            DiscountValue = dto.DiscountValue,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            MaxUses = dto.MaxUses,
            MinOrderAmount = dto.MinOrderAmount,
            IsActive = dto.IsActive
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{couponId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid restaurantId, Guid couponId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new DeleteCouponCommand { RestaurantId = restaurantId, CouponId = couponId, UserId = userId });
        return Ok(result);
    }
}
