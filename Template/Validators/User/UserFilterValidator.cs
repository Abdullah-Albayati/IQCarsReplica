using FluentValidation;
using Template.DTOs;

namespace Template.Validators.User;

public class UserFilterValidator : AbstractValidator<UserFilter>
{
    public UserFilterValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

