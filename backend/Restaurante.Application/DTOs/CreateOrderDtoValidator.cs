using FluentValidation;

namespace Restaurante.Application.DTOs;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required");
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemDtoValidator());
    }
}

public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(x => x.MenuItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
