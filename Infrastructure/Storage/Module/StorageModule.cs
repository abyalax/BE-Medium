
using Medium.Api.Infrastructure.Storage;
using Medium.Api.Infrastructure.Storage.Config;
using Medium.Api.Infrastructure.Storage.Repositories;
using Medium.Api.Infrastructure.Storage.Services;

using Minio;

namespace Medium.Api.Infrastructure.Storage.Module;

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
    services.AddScoped<StorageService>();
    services.AddScoped<ObjectStorageRepository>();
    services.AddMinioStorage();

    return services;
  }
}