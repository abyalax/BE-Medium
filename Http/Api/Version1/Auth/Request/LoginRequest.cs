using FluentValidation;

using Medium.Api.Domain.Auth.Commands;

namespace Medium.Api.Domain.Auth.Validators;

public class LoginRequest : AbstractValidator<LoginCommand>
{
  public LoginRequest()
  {
    RuleFor(request => request.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(request => request.Password)
      .NotEmpty()
      .MinimumLength(8)
      .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
      .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
      .Matches("[0-9]").WithMessage("Password must contain at least one number")
      .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol");
  }
}