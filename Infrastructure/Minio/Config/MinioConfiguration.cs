
namespace Medium.Api.Infrastructure.Minio.Config;

public sealed class MinioConfiguration
{
  public string Endpoint { get; init; } = string.Empty;

  public string AccessKey { get; init; } = string.Empty;

  public string SecretKey { get; init; } = string.Empty;

  public string BucketName { get; init; } = string.Empty;

  public string Region { get; init; } = string.Empty;

  public bool UseSsl { get; init; }
}