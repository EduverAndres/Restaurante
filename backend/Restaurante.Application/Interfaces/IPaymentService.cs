namespace Restaurante.Application.Interfaces;

public interface IPaymentService
{
    Task<(bool success, string transactionId)> ProcessPaymentAsync(decimal amount, string method);
    Task<bool> RefundPaymentAsync(string transactionId);
}
