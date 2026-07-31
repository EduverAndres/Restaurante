using FluentValidation;

namespace Restaurante.Application.DTOs;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(s => s is "Pending" or "Confirmed" or "Preparing" or "Ready" or "Delivered" or "Cancelled");
    }
}
