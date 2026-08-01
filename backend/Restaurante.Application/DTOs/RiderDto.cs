namespace Restaurante.Application.DTOs;

public class RiderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal Rating { get; set; }
    public int RatingsCount { get; set; }
    public DateTime? LastLocationAt { get; set; }
}
