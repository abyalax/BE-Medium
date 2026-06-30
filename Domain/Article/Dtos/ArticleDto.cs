
using Medium.Api.Domain.Storage.Dtos;
using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Enums;

namespace Medium.Api.Domain.Article.Dtos;

public record ArticleDto(
  Guid Id,
  Guid AuthorId,
  string AuthorName,
  string Title,
  string Slug,
  string Content,
  string? Summary,
  string? CoverImageUrl,
  Guid? ThumbnailId,
  ObjectStorageDto? Thumbnail,
  IReadOnlyCollection<ObjectStorageDto> ContentImages,
  ArticleStatus Status,
  DateTime? PublishedAt,
  DateTime? ScheduledAt,
  long ViewCount,
  IReadOnlyCollection<TagDto> Tags,
  DateTime CreatedAt,
  DateTime UpdatedAt
);