using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Restaurante.Application.DTOs;
using Restaurante.Application.Interfaces;
using Restaurante.Domain.Entities;

namespace Restaurante.Infrastructure.Services;

/// <summary>
/// Payment gateway abstraction. Mode (PaymentProvider:Mode) selects the backend:
///   Mock    — fake successful transactions (TXN-...), no network calls.
///   Sandbox — real Wompi calls against https://sandbox.wompi.co/v1.
///   Live    — real Wompi calls against https://production.wompi.co/v1.
/// CASH always settles on delivery: it never touches the gateway.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly string _mode;
    private readonly string _privateKey;
    private readonly string _webhookSecret;
    private readonly HttpClient _httpClient;

    public PaymentService(IConfiguration config, HttpClient httpClient)
    {
        _mode = config["PaymentProvider:Mode"] ?? "Mock";
        _privateKey = config["Wompi:PrivateKey"] ?? string.Empty;
        _webhookSecret = config["Wompi:WebhookSecret"] ?? string.Empty;
        _httpClient = httpClient;
    }

    private string BaseUrl => _mode switch
    {
        "Sandbox" => "https://sandbox.wompi.co/v1",
        "Live" => "https://production.wompi.co/v1",
        _ => string.Empty
    };

    public async Task<PaymentResult> ProcessPaymentAsync(Order order, ProcessPaymentDto dto)
    {
        // Cash on delivery is a legitimate flow: money changes hands at the door.
        if (string.Equals(dto.Method, "CASH", StringComparison.OrdinalIgnoreCase))
        {
            var cashTxnId = $"CASH-{Guid.NewGuid():N}"[..20];
            return new PaymentResult(true, cashTxnId, null, "Paid", "Payment collected on delivery", null);
        }

        if (_mode is not ("Sandbox" or "Live"))
            return ProcessMockPayment(order);

        EnsureCredentials();
        return await ProcessWompiPaymentAsync(order, dto);
    }

    public async Task<PaymentResult> RefundPaymentAsync(Payment payment)
    {
        if (_mode is not ("Sandbox" or "Live"))
        {
            var refundId = $"REF-{Guid.NewGuid():N}"[..20];
            return new PaymentResult(true, refundId, null, "Refunded", "Payment refunded (mock mode)", null);
        }

        EnsureCredentials();

        if (string.IsNullOrWhiteSpace(payment.TransactionId) ||
            payment.TransactionId.StartsWith("CASH-", StringComparison.OrdinalIgnoreCase) ||
            payment.TransactionId.StartsWith("TXN-", StringComparison.OrdinalIgnoreCase))
        {
            return new PaymentResult(false, null, null, "Failed",
                "Transaction was not processed by Wompi and cannot be refunded", null);
        }

        var body = new JsonObject { ["amount_in_cents"] = (int)(payment.Amount * 100) };
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/transactions/{payment.TransactionId}/refund");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _privateKey);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var message = ExtractErrorMessage(raw) ?? $"Wompi refund failed with status {(int)response.StatusCode}";
            return new PaymentResult(false, null, null, "Failed", message, raw);
        }

        return new PaymentResult(true, payment.TransactionId, null, "Refunded", "Payment refunded", raw);
    }

    public Task<bool> VerifyWebhookSignatureAsync(string rawBody, string signature)
    {
        if (string.IsNullOrWhiteSpace(_webhookSecret) || _webhookSecret == "CHANGE_ME")
            throw new UnauthorizedAccessException("Webhook secret not configured");

        if (string.IsNullOrWhiteSpace(signature))
            return Task.FromResult(false);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();
        var provided = signature.ToLowerInvariant();

        if (expected.Length != provided.Length)
            return Task.FromResult(false);

        return Task.FromResult(CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(provided)));
    }

    private static PaymentResult ProcessMockPayment(Order order)
    {
        var txnId = $"TXN-{Guid.NewGuid():N}"[..20];
        return new PaymentResult(true, txnId, $"rest-{order.Id:N}-{Guid.NewGuid():N}", "Paid",
            "Payment processed (mock mode)", null);
    }

    private async Task<PaymentResult> ProcessWompiPaymentAsync(Order order, ProcessPaymentDto dto)
    {
        var reference = $"rest-{order.Id:N}-{Guid.NewGuid():N}";

        var paymentMethod = new JsonObject
        {
            ["type"] = "CARD",
            ["token"] = dto.CardToken
        };
        if (!string.IsNullOrWhiteSpace(dto.AcceptanceToken))
            paymentMethod["acceptance_token"] = dto.AcceptanceToken;

        var body = new JsonObject
        {
            ["amount_in_cents"] = (int)(order.Total * 100),
            ["currency"] = "COP",
            ["reference"] = reference,
            ["payment_method"] = paymentMethod
        };
        if (!string.IsNullOrWhiteSpace(dto.CustomerEmail))
            body["customer_email"] = dto.CustomerEmail;

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/transactions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _privateKey);
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var message = ExtractErrorMessage(raw) ?? $"Wompi request failed with status {(int)response.StatusCode}";
            return new PaymentResult(false, null, reference, "Failed", message, raw);
        }

        using var doc = JsonDocument.Parse(raw);
        var data = doc.RootElement.GetProperty("data");
        var status = data.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
        var transactionId = data.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;

        return status switch
        {
            "APPROVED" => new PaymentResult(true, transactionId, reference, "Paid", "Payment approved", raw),
            "DECLINED" => new PaymentResult(false, transactionId, reference, "Failed", "Payment declined", raw),
            _ => new PaymentResult(true, transactionId, reference, "Pending", "Payment pending confirmation", raw)
        };
    }

    private void EnsureCredentials()
    {
        if (string.IsNullOrWhiteSpace(_privateKey) || _privateKey == "CHANGE_ME")
            throw new InvalidOperationException("Wompi credentials not configured");
    }

    private static string? ExtractErrorMessage(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty("error", out var error))
                return null;

            if (error.TryGetProperty("type", out var type))
                return type.GetString();

            if (error.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
            {
                var parts = new List<string>();
                foreach (var message in messages.EnumerateArray())
                {
                    if (message.ValueKind == JsonValueKind.String)
                        parts.Add(message.GetString() ?? string.Empty);
                    else if (message.TryGetProperty("message", out var text))
                        parts.Add(text.GetString() ?? string.Empty);
                }
                if (parts.Count > 0)
                    return string.Join("; ", parts);
            }
        }
        catch (JsonException)
        {
            // Not JSON: fall through to the generic message.
        }

        return null;
    }
}
