using FluentValidation;

using Medium.Api.Domain.User.Commands;

namespace Medium.Api.Http.Api.Version1.User.Request;

public class UpdateUserRequest : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserRequest()
  {
    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");

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