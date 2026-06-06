/*
 * @description   
 * @since         Saturday, 6 6th 2026, 17:17:24 pm
 * @author        Nguyễn Hữu Hưởng <huongnh@getflycrm.com>
 * @copyright     Copyright (c) 2026, GETFLY VN TECH.,JSC
 * -----
 * Change Log Win: <press Ctrl + alt + c write changelog>
 * Change Log Mac: <press Control + Option + H + H write changelog>
 */


using FluentValidation;
using mini_mes_be.Constants;
using mini_mes_be.DTOs.Auth;

namespace mini_mes_be.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
  public ResetPasswordRequestValidator()
  {
    RuleFor(x => x.email)
        .NotEmpty().WithMessage(ErrorMessages.Validation.EmailRequired)
        .EmailAddress().WithMessage(ErrorMessages.Validation.EmailInvalid);

    RuleFor(x => x.new_password)
        .NotEmpty().WithMessage(ErrorMessages.Validation.PasswordRequired)
        .MinimumLength(8).WithMessage(ErrorMessages.Validation.PasswordMinLength)
        .MaximumLength(128).WithMessage(ErrorMessages.Validation.PasswordMaxLength)
        .Matches(@"[a-z]").WithMessage(ErrorMessages.Validation.PasswordNeedsLowercase)
        .Matches(@"[A-Z]").WithMessage(ErrorMessages.Validation.PasswordNeedsUppercase)
        .Matches(@"[0-9]").WithMessage(ErrorMessages.Validation.PasswordNeedsDigit)
        .Matches(@"[^a-zA-Z0-9]").WithMessage(ErrorMessages.Validation.PasswordNeedsSpecial);

    RuleFor(x => x.confirm_password)
        .Equal(x => x.new_password).WithMessage("Passwords must match");
  }
}
