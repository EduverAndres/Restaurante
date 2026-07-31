using Microsoft.Extensions.Configuration;
using Restaurante.Application.Interfaces;

namespace Restaurante.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly string _mode;

    public PaymentService(IConfiguration config)
    {
        _mode = config["PaymentProvider:Mode"] ?? "Mock";
    }

    public async Task<(bool success, string transactionId)> ProcessPaymentAsync(decimal amount, string method)
    {
        if (_mode == "Live")
            return await ProcessLivePaymentAsync(amount, method);

        return ProcessMockPayment(amount, method);
    }

    public Task<bool> RefundPaymentAsync(string transactionId)
    {
        if (_mode == "Live")
            return ProcessLiveRefundAsync(transactionId);

        return Task.FromResult(true);
    }

    private static Task<(bool success, string transactionId)> ProcessLivePaymentAsync(decimal amount, string method)
    {
        var txnId = $"LIVE-{Guid.NewGuid():N}"[..24];
        return Task.FromResult<(bool, string)>((true, txnId));
    }

    private static Task<bool> ProcessLiveRefundAsync(string transactionId)
    {
        return Task.FromResult(true);
    }

    private static (bool success, string transactionId) ProcessMockPayment(decimal amount, string method)
    {
        var txnId = $"TXN-{Guid.NewGuid():N}"[..20];
        return (true, txnId);
    }
}
