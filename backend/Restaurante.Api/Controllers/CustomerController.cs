using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Addresses.Commands;
using Restaurante.Application.Features.Addresses.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/customer/addresses")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateCustomerAddressDto> _createValidator;
    private readonly IValidator<UpdateCustomerAddressDto> _updateValidator;

    public CustomerController(IMediator mediator, IValidator<CreateCustomerAddressDto> createValidator, IValidator<UpdateCustomerAddressDto> updateValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerAddressDto>>>> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new GetCustomerAddressesQuery { UserId = userId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> Create([FromBody] CreateCustomerAddressDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CustomerAddressDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateCustomerAddressCommand
        {
            UserId = userId,
            Label = dto.Label,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsDefault = dto.IsDefault
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> Update(Guid id, [FromBody] UpdateCustomerAddressDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<CustomerAddressDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UpdateCustomerAddressCommand
        {
            Id = id,
            UserId = userId,
            Label = dto.Label,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsDefault = dto.IsDefault
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new DeleteCustomerAddressCommand { Id = id, UserId = userId });
        return Ok(result);
    }
}
