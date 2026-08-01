using FluentValidation;

namespace Restaurante.Application.DTOs;

public class AssignRiderDto
{
    public Guid? RiderId { get; set; }
}

public class AssignRiderDtoValidator : AbstractValidator<AssignRiderDto>
{
    public AssignRiderDtoValidator()
    {
        RuleFor(x => x.RiderId)
            .Must(r => r is null || r != Guid.Empty)
            .WithMessage("RiderId must be a valid GUID when provided");
    }
}
