namespace Medium.Api.Infrastructure.Storage.Dtos;

using Medium.Api.Enums;

public record ObjectStorageDto(
  Guid Id,
  string ObjectKey,
  string Bucket,
  string MimeType,
  string OriginalName,
  int? Size,
  FileAccessType AccessType,
  string? UploadUrl,
  DateTime CreatedAt
);