using Restaurante.Domain.Common;
using Restaurante.Domain.Enums;

namespace Restaurante.Domain.Entities;

public class Rider : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public VehicleType VehicleType { get; set; }
    public RiderStatus Status { get; set; } = RiderStatus.Available;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal Rating { get; set; } = 0;
    public int RatingsCount { get; set; } = 0;
    public DateTime? LastLocationAt { get; set; }

    public List<Order> Orders { get; set; } = new();
}
