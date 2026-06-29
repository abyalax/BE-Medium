using Microsoft.Extensions.Diagnostics.HealthChecks;

using Microsoft.Extensions.Options;

using Medium.Api.Infrastructure.Settings;

namespace Medium.Api.Infrastructure.Storage;

public class MinioHealthCheck(IMinioClient minioClient, IOptions<AppSettings> settings) : IHealthCheck
{
  private readonly IMinioClient _minioClient = minioClient;
  private readonly AppSettings _settings = settings.Value;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      // Check if we can verify bucket existence to test connectivity
      var bucketName = _settings.Minio.BucketName;
      if (!string.IsNullOrEmpty(bucketName))
      {
        await _minioClient.BucketExistsAsync(bucketName, cancellationToken);
      }

      return HealthCheckResult.Healthy("MinIO connection is healthy");
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("MinIO connection failed", ex);
    }
  }
}
