using FluentValidation;

using Medium.Api.Domain.Comment.Commands;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class CreateCommentRequest : AbstractValidator<CreateCommentCommand>
{
  public CreateCommentRequest()
  {
    RuleFor(x => x.ArticleId)
      .NotEmpty().WithMessage("Article ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");

    RuleFor(x => x.Content)
      .NotEmpty().WithMessage("Comment content is required")
      .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters");
  }
}