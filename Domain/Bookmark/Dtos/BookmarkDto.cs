

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.Bookmark.Dtos;

public record BookmarkDto(
  Guid Id,
  Guid UserId,
  Guid ArticleId,
  string? ArticleTitle,
  string? ArticleSlug,
  UserDto? User,
  ArticleDto? Article,
  DateTime CreatedAt
);