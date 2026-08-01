using FluentValidation;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.DTOs;

public class UpdateRiderStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateRiderStatusDtoValidator : AbstractValidator<UpdateRiderStatusDto>
{
    public UpdateRiderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => Enum.TryParse<RiderStatus>(s, true, out _))
            .WithMessage("Invalid status value. Valid values: Available, Busy, Offline");
    }
}
