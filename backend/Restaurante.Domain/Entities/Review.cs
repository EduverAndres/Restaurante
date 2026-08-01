using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class Review : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
