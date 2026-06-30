using FluentValidation;

using Medium.Api.Domain.Comment.Queries;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class GetCommentByIdRequest : AbstractValidator<GetCommentByIdQuery>
{
  public GetCommentByIdRequest()
  {
    RuleFor(x => x.CommentId)
      .NotEmpty().WithMessage("Comment ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Comment ID must be valid GUID");
  }
}