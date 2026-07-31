using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[] Images { get; set; } = Array.Empty<string>();
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public int PreparationTime { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();

    public MenuItem(string name, decimal price, Guid restaurantId, Guid categoryId)
    {
        Name = name;
        Price = price;
        RestaurantId = restaurantId;
        CategoryId = categoryId;
        IsAvailable = true;
    }
}
