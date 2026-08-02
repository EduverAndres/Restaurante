using FluentValidation;

namespace Restaurante.Application.DTOs;

public class UpdateDeliverySettingsDto
{
    public decimal DeliveryFee { get; set; }
    public double? RadiusKm { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? EstimatedPrepTimeMinutes { get; set; }
}

public class UpdateDeliverySettingsDtoValidator : AbstractValidator<UpdateDeliverySettingsDto>
{
    public UpdateDeliverySettingsDtoValidator()
    {
        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("DeliveryFee must be greater than or equal to 0");
        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("MinOrderAmount must be greater than or equal to 0");
        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("RadiusKm must be greater than 0")
            .When(x => x.RadiusKm.HasValue);
        RuleFor(x => x.EstimatedPrepTimeMinutes)
            .GreaterThan(0).WithMessage("EstimatedPrepTimeMinutes must be greater than 0")
            .When(x => x.EstimatedPrepTimeMinutes.HasValue);
    }
}
