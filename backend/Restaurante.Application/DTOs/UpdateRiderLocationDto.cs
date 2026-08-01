using FluentValidation;

namespace Restaurante.Application.DTOs;

public class UpdateRiderLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateRiderLocationDtoValidator : AbstractValidator<UpdateRiderLocationDto>
{
    public UpdateRiderLocationDtoValidator()
    {
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
