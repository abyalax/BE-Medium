using FluentValidation;

using Medium.Api.Domain.Bookmark.Commands;

namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

public class DeleteBookmarkByArticleValidator : AbstractValidator<DeleteBookmarkByArticleCommand>
{
  public DeleteBookmarkByArticleValidator()
  {
    RuleFor(x => x.ArticleId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Bookmark ID must be valid GUID");

    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");

  }
}