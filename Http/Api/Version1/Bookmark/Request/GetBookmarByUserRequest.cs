namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

using FluentValidation;

using Medium.Api.Domain.Bookmark.Queries;

public class GetBookmarkByUserRequest : AbstractValidator<GetBookmarkByUserQuery>
{
  public GetBookmarkByUserRequest()
  {
    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");
  }
}