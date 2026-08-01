namespace Restaurante.Application.DTOs;

public class PaymentWebhookDto
{
    public string Event { get; set; } = string.Empty;
    public PaymentWebhookDataDto Data { get; set; } = new();
}

public class PaymentWebhookDataDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public long AmountInCents { get; set; }
}
