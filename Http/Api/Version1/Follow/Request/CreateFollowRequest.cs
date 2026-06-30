using FluentValidation;

using Medium.Api.Domain.Follow.Commands;

namespace Medium.Api.Http.Api.Version1.Follow.Request;

public class CreateFollowValidator : AbstractValidator<CreateFollowCommand>
{
  public CreateFollowValidator()
  {
    RuleFor(x => x.Request.FollowingId)
      .NotEmpty().WithMessage("FollowingId is required")
      .NotEqual(x => x.FollowerId).WithMessage("You cannot follow yourself")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");
  }
}