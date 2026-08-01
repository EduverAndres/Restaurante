using FluentValidation;

namespace Restaurante.Application.DTOs;

public class CustomerAddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCustomerAddressDto
{
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCustomerAddressDto
{
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCustomerAddressDtoValidator : AbstractValidator<CreateCustomerAddressDto>
{
    public CreateCustomerAddressDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180");
    }
}

public class UpdateCustomerAddressDtoValidator : AbstractValidator<UpdateCustomerAddressDto>
{
    public UpdateCustomerAddressDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180");
    }
}
