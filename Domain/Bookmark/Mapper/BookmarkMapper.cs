using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.User.Dtos;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Mapper;

public class BookmarkMapper
{
  public static BookmarkDto ToResponse(BookmarkModel bookmark)
  {
    // Map Article if available
    var articleDto = bookmark.Article == null
        ? null
        : ArticleMapper.ToResponse(bookmark.Article);

    // Map User if available
    var userDto = bookmark.User == null
        ? null
        : new UserDto(
            bookmark.User.Id,
            bookmark.User.Name,
            bookmark.User.Email,
            bookmark.User.Bio,
            bookmark.User.AvatarUrl,
            [],
            bookmark.User.CreatedAt,
            bookmark.User.UpdatedAt
          );

    return new BookmarkDto(
        bookmark.Id,
        bookmark.UserId,
        bookmark.ArticleId,
        bookmark.Article?.Title,
        bookmark.Article?.Slug,
        userDto,
        articleDto,
        bookmark.CreatedAt
    );
  }
}