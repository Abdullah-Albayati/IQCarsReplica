using Template.DTOs;
using FluentValidation;

namespace Template.Validators.Car;

public class CarFormValidator : AbstractValidator<CarForm>
{
    public CarFormValidator()
    {
        // TODO: Add validation rules
        // Example: RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
