using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }

    public OrderItem(Guid orderId, Guid menuItemId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        MenuItemId = menuItemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
