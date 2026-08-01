using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Features.Payments.Commands;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<RefundPaymentDto> _refundValidator;

    public PaymentsController(IMediator mediator, IValidator<RefundPaymentDto> refundValidator)
    {
        _mediator = mediator;
        _refundValidator = refundValidator;
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Checkout([FromBody] ProcessPaymentDto dto)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ProcessPaymentCommand
        {
            OrderId = dto.OrderId,
            CustomerId = customerId,
            Method = dto.Method,
            CardToken = dto.CardToken,
            AcceptanceToken = dto.AcceptanceToken,
            CustomerEmail = dto.CustomerEmail
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/refund")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Refund(Guid id)
    {
        var dto = new RefundPaymentDto { PaymentId = id };
        var validation = await _refundValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<PaymentDto>.Fail("Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToList()));

        var result = await _mediator.Send(new RefundPaymentCommand { PaymentId = id });
        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();
        var signature = Request.Headers["x-signature"].ToString();
        var payload = JsonSerializer.Deserialize<PaymentWebhookDto>(rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PaymentWebhookDto();

        await _mediator.Send(new PaymentWebhookCommand
        {
            RawBody = rawBody,
            Signature = signature,
            Payload = payload
        });

        return Ok(ApiResponse<bool>.Ok(true));
    }
}
