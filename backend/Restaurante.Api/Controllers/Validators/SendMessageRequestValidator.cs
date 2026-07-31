using FluentValidation;

namespace Restaurante.Api.Controllers;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
