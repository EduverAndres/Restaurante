using FluentValidation;

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
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
    public List<BusinessHourDto> BusinessHours { get; set; } = new();
}

public class CreateRestaurantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
}

public class UpdateRestaurantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? CoverImage { get; set; }
    public string? ThemeConfig { get; set; }
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public decimal? DeliveryFee { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
}

public class CreateRestaurantDtoValidator : AbstractValidator<CreateRestaurantDto>
{
    public CreateRestaurantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(x => x.Slug)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers and dashes")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("Phone must be a valid phone number")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("DeliveryFee must be greater than or equal to 0");
        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("MinOrderAmount must be greater than or equal to 0");
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);
        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("RadiusKm must be greater than 0")
            .When(x => x.RadiusKm.HasValue);
        RuleFor(x => x.EstimatedPrepTimeMinutes)
            .GreaterThan(0).WithMessage("EstimatedPrepTimeMinutes must be greater than 0")
            .When(x => x.EstimatedPrepTimeMinutes.HasValue);
    }
}

public class UpdateRestaurantDtoValidator : AbstractValidator<UpdateRestaurantDto>
{
    public UpdateRestaurantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(x => x.Slug)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers and dashes")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
        RuleFor(x => x.Phone)
            .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("Phone must be a valid phone number")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("DeliveryFee must be greater than or equal to 0")
            .When(x => x.DeliveryFee.HasValue);
        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("MinOrderAmount must be greater than or equal to 0")
            .When(x => x.MinOrderAmount.HasValue);
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90")
            .When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180")
            .When(x => x.Longitude.HasValue);
        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("RadiusKm must be greater than 0")
            .When(x => x.RadiusKm.HasValue);
        RuleFor(x => x.EstimatedPrepTimeMinutes)
            .GreaterThan(0).WithMessage("EstimatedPrepTimeMinutes must be greater than 0")
            .When(x => x.EstimatedPrepTimeMinutes.HasValue);
    }
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
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
