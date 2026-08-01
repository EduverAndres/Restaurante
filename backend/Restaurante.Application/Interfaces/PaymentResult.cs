namespace Restaurante.Application.Interfaces;

public record PaymentResult(
    bool Success,
    string? TransactionId,
    string? Reference,
    string Status,
    string Message,
    string? RawResponse);
