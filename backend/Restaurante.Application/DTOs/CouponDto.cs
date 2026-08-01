using FluentValidation;

namespace Restaurante.Application.DTOs;

public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public Guid? RestaurantId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public int TimesUsed { get; set; }
    public decimal MinOrderAmount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinOrderAmount { get; set; }
}

public class UpdateCouponDto
{
    public decimal DiscountValue { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);
        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .Must(d => d == "Percentage" || d == "Fixed")
            .WithMessage("DiscountType must be Percentage or Fixed");
        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("DiscountValue must be greater than 0");
        RuleFor(x => x.ValidFrom).NotEmpty();
        RuleFor(x => x.ValidUntil)
            .NotEmpty()
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("ValidUntil must be after ValidFrom");
        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue)
            .WithMessage("MaxUses must be greater than 0");
        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue)
            .WithMessage("MinOrderAmount must be greater than or equal to 0");
    }
}

public class UpdateCouponDtoValidator : AbstractValidator<UpdateCouponDto>
{
    public UpdateCouponDtoValidator()
    {
        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("DiscountValue must be greater than 0");
        RuleFor(x => x.ValidFrom).NotEmpty();
        RuleFor(x => x.ValidUntil)
            .NotEmpty()
            .GreaterThan(x => x.ValidFrom)
            .WithMessage("ValidUntil must be after ValidFrom");
        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue)
            .WithMessage("MaxUses must be greater than 0");
        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue)
            .WithMessage("MinOrderAmount must be greater than or equal to 0");
    }
}
