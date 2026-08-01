using FluentValidation;

namespace Restaurante.Application.DTOs;

public class ApplyCouponDto
{
    public string Code { get; set; } = string.Empty;
}

public class ApplyCouponDtoValidator : AbstractValidator<ApplyCouponDto>
{
    public ApplyCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Coupon code is required");
        RuleFor(x => x.Code).MaximumLength(50).WithMessage("Coupon code must be at most 50 characters");
    }
}
