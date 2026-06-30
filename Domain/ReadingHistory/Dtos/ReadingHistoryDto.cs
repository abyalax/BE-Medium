
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.ReadingHistory.Dtos;

public record ReadingHistoryDto(
  Guid Id,
  Guid UserId,
  Guid ArticleId,
  int DurationSeconds,
  UserDto? User,
  ArticleDto? Article,
  DateTime ReadAt
);