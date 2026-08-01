using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class CustomerAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; } = false;
}
