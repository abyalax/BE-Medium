using FluentValidation;

using Medium.Api.Domain.Follow.Commands;

namespace Medium.Api.Http.Api.Version1.Follow.Request;

public class UnfollowValidator : AbstractValidator<UnfollowCommand>
{
  public UnfollowValidator()
  {
    RuleFor(x => x.FollowerId)
      .NotEmpty().WithMessage("FollowerId is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Follower ID must be valid GUID");

    RuleFor(x => x.FollowingId)
      .NotEmpty().WithMessage("FollowingId is required")
      .NotEqual(x => x.FollowerId).WithMessage("You cannot unfollow yourself")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Following ID must be valid GUID");
  }
}