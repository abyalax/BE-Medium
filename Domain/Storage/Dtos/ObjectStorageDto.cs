
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Enums;

namespace Medium.Api.Domain.Storage.Dtos;

public record ObjectStorageDto(
  Guid AuthorId,
  Guid? ArticleId,
  string Bucket,
  string ObjectKey,
  string MimeType,
  string OriginalName,
  int? Size,
  FileAccessType AccessTypes,
  UserDto? Author,
  ArticleDto? Article
);