using FluentValidation;

namespace Restaurante.Application.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RestaurantReviewsDto
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int Count { get; set; }
}

public class CreateReviewDto
{
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Comment))
            .WithMessage("Comment must not exceed 1000 characters");
    }
}
