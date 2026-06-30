namespace Medium.Api.Infrastructure.Storage;

public static class MinioClientFactory
{
  public static IServiceCollection AddMinioStorage(this IServiceCollection services)
  {
    services.AddSingleton<IMinioClient, MinioClientWrapper>();
    services.AddScoped<IStorageService, S3StorageService>();
    return services;
  }
}