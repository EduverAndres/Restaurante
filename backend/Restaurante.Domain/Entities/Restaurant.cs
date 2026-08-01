using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class Restaurant : BaseEntity
{
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal DeliveryFee { get; set; } = 0;
    public decimal MinOrderAmount { get; set; } = 0;
    public int? EstimatedPrepTimeMinutes { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<MenuItem> MenuItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<BusinessHour> BusinessHours { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Coupon> Coupons { get; set; } = new();

    public Restaurant(string name, string slug, Guid ownerId)
    {
        Name = name;
        Slug = slug;
        OwnerId = ownerId;
        IsActive = true;
    }
}
