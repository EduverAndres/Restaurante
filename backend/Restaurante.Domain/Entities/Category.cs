using Restaurante.Domain.Common;

namespace Restaurante.Domain.Entities;

public class Category : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }

    public List<MenuItem> MenuItems { get; set; } = new();

    public Category(string name, Guid restaurantId)
    {
        Name = name;
        RestaurantId = restaurantId;
    }
}
