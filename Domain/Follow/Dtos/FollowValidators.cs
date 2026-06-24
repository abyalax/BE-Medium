using FluentValidation;

using Medium.Api.Domain.Follow.Dtos;

namespace Medium.Api.Domain.Follow.Dtos;

public class FollowRequestValidator : AbstractValidator<FollowRequest>
{
  public FollowRequestValidator()
  {
    RuleFor(x => x.FollowingId)
        .NotEmpty().WithMessage("Following ID is required");
  }
}