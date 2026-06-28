using FluentValidation;

using Medium.Api.Domain.Auth.Commands;

namespace Medium.Api.Domain.Auth.Validators;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
  public RegisterValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Name is required")
        .MinimumLength(2).WithMessage("Name must be at least 2 characters")
        .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required")
        .EmailAddress().WithMessage("Email must be a valid email address");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password is required")
      .MinimumLength(8)
      .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
      .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
      .Matches("[0-9]").WithMessage("Password must contain at least one number")
      .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol");

    RuleFor(x => x.Bio)
      .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");

    RuleFor(x => x.AvatarUrl)
      .MaximumLength(2048)
      .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
      .When(request => !string.IsNullOrWhiteSpace(request.AvatarUrl))
      .WithMessage("AvatarUrl must be a valid absolute URL");
  }
}