using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? Reference { get; set; }

    public Payment(Guid orderId, decimal amount, string method)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Pending;
    }
}
