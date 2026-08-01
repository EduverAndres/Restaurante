namespace Restaurante.Application.DTOs;

public class MenuItemContext
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public MenuItemContext(Guid id, string name, decimal price, string? description, string categoryName)
    {
        Id = id;
        Name = name;
        Price = price;
        Description = description;
        CategoryName = categoryName;
    }
}
