using FluentValidation;

using Medium.Api.Http.Api.Version1.Users;

namespace Medium.Api.Domain.User.Dtos;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
  public CreateUserRequestValidator()
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

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
  public UpdateUserRequestValidator()
  {
    RuleFor(request => request.Name)
        .NotEmpty()
        .MaximumLength(150);

    RuleFor(request => request.Email)
        .NotEmpty()
        .EmailAddress()
        .MaximumLength(255);

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

public sealed class AssignUserRolesRequestValidator : AbstractValidator<UserEndpoints.AssignUserRolesRequest>
{
  public AssignUserRolesRequestValidator()
  {
    RuleFor(request => request.RoleIds)
        .NotEmpty()
        .Must(roleIds => roleIds.Distinct().Count() == roleIds.Count)
        .WithMessage("RoleIds must not contain duplicates");
  }
}