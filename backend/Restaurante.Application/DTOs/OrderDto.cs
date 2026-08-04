namespace Restaurante.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public Guid? CouponId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string CustomerName { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class CreateOrderDto
{
    public Guid RestaurantId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class CreateOrderItemDto
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}
