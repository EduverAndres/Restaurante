using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public Guid? AiConversationId { get; set; }
    public AIConversation? AiConversation { get; set; }
    public string? Notes { get; set; }
    public string? DeliveryAddress { get; set; }
    public Guid? RiderId { get; set; }
    public Rider? Rider { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public Review? Review { get; set; }

    public Order(Guid customerId, Guid restaurantId)
    {
        CustomerId = customerId;
        RestaurantId = restaurantId;
        Status = OrderStatus.Pending;
        PaymentStatus = Enums.PaymentStatus.Pending;
    }
}
