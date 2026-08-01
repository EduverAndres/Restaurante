namespace Restaurante.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    Ready,
    AssignedToRider,
    OutForDelivery,
    Delivered,
    Cancelled
}
