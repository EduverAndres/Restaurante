using FluentValidation;

namespace Restaurante.Application.DTOs;

public class CreateMenuItemDtoValidator : AbstractValidator<CreateMenuItemDto>
{
    public CreateMenuItemDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PreparationTime).GreaterThanOrEqualTo(0);
    }
}
