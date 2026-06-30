using FluentValidation;

using Medium.Api.Domain.Comment.Commands;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class DeleteCommentRequest : AbstractValidator<DeleteCommentCommand>
{
  public DeleteCommentRequest()
  {
    RuleFor(x => x.CommentId)
      .NotEmpty().WithMessage("Comment ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Comment ID must be valid GUID");
  }
}