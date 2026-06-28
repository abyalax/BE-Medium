namespace Medium.Api.Domain.Storage.Events;

public record FileUploadedEvent(string ObjectName, string BucketName, string ContentType);