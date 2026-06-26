
using Medium.Api.Infrastructure.Minio.Config;
using Medium.Api.Infrastructure.Minio.Services;

using Minio;

namespace Medium.Api.Infrastructure.Minio.Module;

public static class MinioModule
{
  public static IServiceCollection AddMinioInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {

    var minioConfig = configuration
      .GetSection("Minio")
      .Get<MinioConfiguration>()
      ?? throw new InvalidOperationException("Minio configuration is missing.");

    var minio = new MinioClient()
      .WithEndpoint(minioConfig.Endpoint)
      .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey)
      .WithSSL(minioConfig.UseSsl)
      .WithRegion(minioConfig.Region)
      .Build();

    services.AddSingleton(minio);
    services.AddSingleton(minioConfig);
    services.AddScoped<MinioService>();

    return services;
  }
}