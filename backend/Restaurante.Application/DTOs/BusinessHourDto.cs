using FluentValidation;

namespace Restaurante.Application.DTOs;

public class BusinessHourDto
{
    public int DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class UpdateBusinessHoursDto
{
    public List<BusinessHourDto> Hours { get; set; } = new();
}

public class UpdateBusinessHoursDtoValidator : AbstractValidator<UpdateBusinessHoursDto>
{
    public UpdateBusinessHoursDtoValidator()
    {
        RuleFor(x => x.Hours)
            .Must(h => h.Count <= 7).WithMessage("At most 7 business hours allowed");
        RuleFor(x => x.Hours)
            .Must(h => h.Select(x => x.DayOfWeek).Distinct().Count() == h.Count)
            .WithMessage("Each day of week can only appear once");
        RuleForEach(x => x.Hours).ChildRules(hours =>
        {
            hours.RuleFor(x => x.DayOfWeek)
                .InclusiveBetween(0, 6).WithMessage("DayOfWeek must be between 0 and 6");
            hours.RuleFor(x => x.OpenTime)
                .NotEqual(x => x.CloseTime).WithMessage("OpenTime must be different from CloseTime")
                .When(x => !x.IsClosed);
        });
    }
}
