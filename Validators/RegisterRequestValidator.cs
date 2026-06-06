using FluentValidation;
using mini_mes_be.Constants;
using mini_mes_be.DTOs.Auth;

namespace mini_mes_be.Validators;

/// <summary>
/// Validates the registration payload.
/// Password must contain at least one lowercase letter, one uppercase letter,
/// one digit, and one special character.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.username)
            .NotEmpty().WithMessage(ErrorMessages.Validation.UsernameRequired)
            .MinimumLength(3).WithMessage(ErrorMessages.Validation.UsernameMinLength)
            .MaximumLength(100).WithMessage(ErrorMessages.Validation.UsernameMaxLength);

        RuleFor(x => x.email)
            .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
            .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalid);

        RuleFor(x => x.password)
            .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired)
            .MinimumLength(8).WithMessage(ErrorMessages.Validation.PasswordMinLength)
            .MaximumLength(128).WithMessage(ErrorMessages.Validation.PasswordMaxLength)
            .Matches(@"[a-z]").WithMessage(ErrorMessages.Validation.PasswordNeedsLowercase)
            .Matches(@"[A-Z]").WithMessage(ErrorMessages.Validation.PasswordNeedsUppercase)
            .Matches(@"[0-9]").WithMessage(ErrorMessages.Validation.PasswordNeedsDigit)
            .Matches(@"[^a-zA-Z0-9]").WithMessage(ErrorMessages.Validation.PasswordNeedsSpecial);
    }
}

/// <summary>
/// Validates the login payload (email + password).
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
            .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalid);

        RuleFor(x => x.password)
            .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired);
    }
}
