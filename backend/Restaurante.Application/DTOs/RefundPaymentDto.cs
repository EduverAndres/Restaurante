using FluentValidation;

namespace Restaurante.Application.DTOs;

public class RefundPaymentDto
{
    public Guid PaymentId { get; set; }
}

public class RefundPaymentDtoValidator : AbstractValidator<RefundPaymentDto>
{
    public RefundPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
