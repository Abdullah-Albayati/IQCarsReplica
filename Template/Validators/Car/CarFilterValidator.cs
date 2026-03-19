using Template.DTOs;
using FluentValidation;

namespace Template.Validators.Car;

public class CarFilterValidator : AbstractValidator<CarFilter>
{
    public CarFilterValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        // TODO: Add more validation rules
    }
}
