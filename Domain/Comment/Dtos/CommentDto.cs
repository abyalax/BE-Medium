
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.Comment.Dtos;

public record CommentDto(
  Guid Id,
  Guid UserId,
  Guid ArticleId,
  string Content,
  UserDto? User,
  ArticleDto? Article,
  DateTime CreatedAt,
  DateTime UpdatedAt
);