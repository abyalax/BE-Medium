namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

using FluentValidation;

using Medium.Api.Domain.Bookmark.Queries;

public class GetBookmarkByIdRequest : AbstractValidator<GetBookmarByIdQuery>
{
  public GetBookmarkByIdRequest()
  {
    RuleFor(x => x.BookmarkId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Bookmark ID must be valid GUID");
  }
}