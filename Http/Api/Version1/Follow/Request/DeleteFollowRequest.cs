using FluentValidation;

using Medium.Api.Domain.Follow.Commands;

namespace Medium.Api.Http.Api.Version1.Follow.Request;

public class DeleteFollowValidator : AbstractValidator<DeleteFollowCommand>
{
  public DeleteFollowValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("Id is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");

    RuleFor(x => x.CurrentUserId)
      .NotEmpty().WithMessage("CurrentUserId is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");
  }
}