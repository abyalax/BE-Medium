namespace Medium.Api.Domain.Storage.Commands;

public record UploadedFileResponse(
    string ObjectName,
    string PresignedUrl,
    long FileSize,
    DateTime UploadedAt
);