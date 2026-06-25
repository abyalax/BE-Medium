using FluentValidation;

using Medium.Api.Http.Api.Version1.Auth;

namespace Medium.Api.Domain.Auth.DTOs;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
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

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one symbol");

        RuleFor(request => request.Bio)
            .MaximumLength(500)
            .When(request => request.Bio is not null);

        RuleFor(request => request.AvatarUrl)
            .MaximumLength(2048)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(request => !string.IsNullOrWhiteSpace(request.AvatarUrl))
            .WithMessage("AvatarUrl must be a valid absolute URL");
    }
}

public sealed class CreateRoleRequestValidator : AbstractValidator<RoleEndpoints.CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}

public sealed class CreatePermissionRequestValidator : AbstractValidator<PermissionEndpoints.CreatePermissionRequest>
{
    public CreatePermissionRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty()
            .MaximumLength(150)
            .Matches("^[a-z0-9_]+\\.[a-z0-9_]+(?:_[a-z0-9_]+)*$")
            .WithMessage("Code must use module.action format");

        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}