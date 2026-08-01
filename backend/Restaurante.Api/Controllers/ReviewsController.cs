using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Reviews.Commands;
using Restaurante.Application.Features.Reviews.Queries;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateReviewDto> _createValidator;

    public ReviewsController(IMediator mediator, IValidator<CreateReviewDto> createValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<RestaurantReviewsDto>>> GetAll(Guid restaurantId)
    {
        var result = await _mediator.Send(new GetRestaurantReviewsQuery { RestaurantId = restaurantId });
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> Create(Guid restaurantId, [FromBody] CreateReviewDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<ReviewDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateReviewCommand
        {
            RestaurantId = restaurantId,
            CustomerId = customerId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
