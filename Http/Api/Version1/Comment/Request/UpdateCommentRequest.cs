using FluentValidation;

using Medium.Api.Domain.Comment.Commands;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class UpdateCommentRequest : AbstractValidator<UpdateCommentCommand>
{
  public UpdateCommentRequest()
  {
    RuleFor(x => x.CommentId)
      .NotEmpty().WithMessage("Comment ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Comment ID must be valid GUID");

    RuleFor(x => x.Content)
      .NotEmpty().WithMessage("Comment content is required")
      .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");
  }
}