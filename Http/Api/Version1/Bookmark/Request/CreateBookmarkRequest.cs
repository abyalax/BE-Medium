using FluentValidation;

using Medium.Api.Domain.Bookmark.Commands;

namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

public class CreateBookmarkValidator : AbstractValidator<CreateBookmarkCommand>
{
  public CreateBookmarkValidator()
  {
    RuleFor(x => x.ArticleId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");

    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");
  }
}