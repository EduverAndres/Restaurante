namespace Restaurante.Application.DTOs;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
    public Guid OwnerId { get; set; }
}

public class CreateRestaurantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
}

public class UpdateRestaurantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
}

public class RestaurantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public bool IsActive { get; set; }
}
