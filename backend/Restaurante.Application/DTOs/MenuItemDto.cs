namespace Restaurante.Application.DTOs;

public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[] Images { get; set; } = Array.Empty<string>();
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int PreparationTime { get; set; }
}

public class CreateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[]? Images { get; set; }
    public Guid CategoryId { get; set; }
    public int PreparationTime { get; set; }
}

public class UpdateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string[]? Images { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
    public int PreparationTime { get; set; }
}
