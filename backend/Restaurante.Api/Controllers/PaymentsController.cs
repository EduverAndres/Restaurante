using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Process([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var (success, transactionId) = await _paymentService.ProcessPaymentAsync(request.Amount, request.Method);
            if (!success)
                return BadRequest(ApiResponse<PaymentDto>.Fail("Payment processing failed"));

            var payment = new PaymentDto
            {
                Amount = request.Amount,
                Method = request.Method,
                Status = "Paid",
                TransactionId = transactionId,
                CreatedAt = DateTime.UtcNow
            };
            return Ok(ApiResponse<PaymentDto>.Ok(payment, "Payment processed"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaymentDto>.Fail(ex.Message));
        }
    }
}

public class ProcessPaymentRequest
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
}
