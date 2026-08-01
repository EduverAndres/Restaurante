using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Riders.Commands;
using Restaurante.Application.Features.Riders.Queries;
using Restaurante.Domain.Enums;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/rider")]
[Authorize(Roles = "Delivery")]
public class RiderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<UpdateRiderStatusDto> _statusValidator;
    private readonly IValidator<UpdateRiderLocationDto> _locationValidator;

    public RiderController(IMediator mediator, IValidator<UpdateRiderStatusDto> statusValidator, IValidator<UpdateRiderLocationDto> locationValidator)
    {
        _mediator = mediator;
        _statusValidator = statusValidator;
        _locationValidator = locationValidator;
    }

    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<RiderDto>>> UpdateStatus([FromBody] UpdateRiderStatusDto dto)
    {
        var validation = await _statusValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RiderDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        if (!Enum.TryParse<RiderStatus>(dto.Status, true, out var status))
            return BadRequest(ApiResponse<RiderDto>.Fail("Invalid status value"));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateRiderStatusCommand { UserId = userId, Status = status });
        return Ok(result);
    }

    [HttpPut("location")]
    public async Task<ActionResult<ApiResponse<RiderDto>>> UpdateLocation([FromBody] UpdateRiderLocationDto dto)
    {
        var validation = await _locationValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<RiderDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateRiderLocationCommand
        {
            UserId = userId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        });
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrders()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetRiderOrdersQuery { UserId = userId });
        return Ok(result);
    }
}
