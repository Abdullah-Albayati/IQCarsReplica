using FluentValidation;
using Template.DTOs;

namespace Template.Validators.User;

public class UserFormValidator : AbstractValidator<UserForm>
{
    public UserFormValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username cannot be empty");
        RuleFor(x => x.Username).Length(3, 20).WithMessage("Username must be between 3 and 20 characters");
        RuleFor(x => x.Username).Must(HasNoSpecialCharacters).WithMessage("Username cannot contain special characters");

        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Password).Must(HasSpecialCharacters).WithMessage("Please enter at least one special character");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email cannot be empty");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Please enter a valid email address");
        RuleFor(x => x.Email).Must(EndsWithValidSuffix).WithMessage("Please enter a valid, personal email address");
    }

    private static bool HasNoSpecialCharacters(string value)
    {
        return !string.IsNullOrEmpty(value) && value.All(char.IsLetterOrDigit);
    }

    private static bool HasSpecialCharacters(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Any(ch => !char.IsLetterOrDigit(ch));
    }

    private static bool EndsWithValidSuffix(string value)
    {
        return value.EndsWith(".com", StringComparison.OrdinalIgnoreCase);
    }
}

