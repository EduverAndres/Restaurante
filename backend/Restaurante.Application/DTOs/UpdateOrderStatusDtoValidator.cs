using FluentValidation;
using Restaurante.Domain.Enums;

namespace Restaurante.Application.DTOs;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => Enum.TryParse<OrderStatus>(s, true, out _))
            .WithMessage("Invalid status value. Valid values: Pending, Confirmed, Preparing, Ready, AssignedToRider, OutForDelivery, Delivered, Cancelled");
    }
}
