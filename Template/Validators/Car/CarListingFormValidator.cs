using Template.DTOs;
using FluentValidation;

namespace Template.Validators.Car;

public class CarListingFormValidator : AbstractValidator<CarListingForm>
{
    public CarListingFormValidator()
    {
        // TODO: Add validation rules
        // Example: RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
