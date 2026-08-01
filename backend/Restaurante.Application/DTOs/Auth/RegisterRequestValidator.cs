using FluentValidation;

namespace Restaurante.Application.DTOs.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty().Must(r =>
        {
            var normalized = r.ToLowerInvariant();
            return normalized is "customer" or "restaurantowner" or "restaurant"
                or "delivery" or "rider" or "platformadmin" or "admin" or "platform_admin";
        }).WithMessage("Invalid role. Valid roles: Customer, RestaurantOwner, Delivery, PlatformAdmin");
    }
}
