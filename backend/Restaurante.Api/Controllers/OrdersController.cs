using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Orders.Commands;
using Restaurante.Application.Features.Orders.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpGet("customer")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetByCustomer()
    {
        try
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetOrdersByCustomerQuery { CustomerId = customerId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<OrderDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetByRestaurant(Guid restaurantId)
    {
        try
        {
            var result = await _mediator.Send(new GetOrdersByRestaurantQuery { RestaurantId = restaurantId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<OrderDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateOrderCommand
            {
                CustomerId = customerId,
                RestaurantId = dto.RestaurantId,
                Items = dto.Items,
                Notes = dto.Notes
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var command = new UpdateOrderStatusCommand
            {
                OrderId = id,
                Status = dto.Status,
                ChangedBy = userId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/apply-coupon")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ApplyCoupon(Guid id, [FromBody] ApplyCouponDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ApplyCouponCommand
        {
            OrderId = id,
            UserId = userId,
            Code = dto.Code
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
