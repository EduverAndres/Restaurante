using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string ChangedBy { get; set; }

    public OrderStatusHistory(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus, string changedBy)
    {
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedBy = changedBy;
    }
}
