using Restaurante.Application.DTOs;
using Restaurante.Domain.Entities;

namespace Restaurante.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Order order, ProcessPaymentDto dto);
    Task<PaymentResult> RefundPaymentAsync(Payment payment);
    Task<bool> VerifyWebhookSignatureAsync(string rawBody, string signature);
}
