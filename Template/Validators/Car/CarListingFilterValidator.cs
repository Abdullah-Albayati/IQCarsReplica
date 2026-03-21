using Template.DTOs;
using FluentValidation;

namespace Template.Validators.Car;

public class CarListingFilterValidator : AbstractValidator<CarListingFilter>
{
    public CarListingFilterValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        // TODO: Add more validation rules
    }
}
