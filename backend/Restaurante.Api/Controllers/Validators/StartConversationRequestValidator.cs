using FluentValidation;

namespace Restaurante.Api.Controllers;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.InitialMessage).MaximumLength(2000);
    }
}
