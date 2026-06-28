using Microsoft.Extensions.DependencyInjection;

namespace Medium.Api.Infrastructure.Compression;

public static class CompressionExtensions
{
  public static IServiceCollection AddCompression(this IServiceCollection services)
  {
    services.AddSingleton<ICompressionService, GZipCompressionService>();
    return services;
  }
}