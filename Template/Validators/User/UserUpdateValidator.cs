using FluentValidation;
using Template.DTOs;

namespace Template.Validators.User;

public class UserUpdateValidator : AbstractValidator<UserUpdate>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.Username).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Username));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

