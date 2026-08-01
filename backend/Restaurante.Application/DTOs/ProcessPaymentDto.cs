using FluentValidation;

namespace Restaurante.Application.DTOs;

public class ProcessPaymentDto
{
    public Guid OrderId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? CardToken { get; set; }
    public string? AcceptanceToken { get; set; }
    public string? CustomerEmail { get; set; }
}

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(m => m == "CARD" || m == "CASH")
            .WithMessage("Method must be CARD or CASH");
        RuleFor(x => x.CardToken)
            .NotEmpty()
            .When(x => x.Method == "CARD")
            .WithMessage("CardToken is required for card payments");
        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail))
            .WithMessage("CustomerEmail must be a valid email address");
    }
}
