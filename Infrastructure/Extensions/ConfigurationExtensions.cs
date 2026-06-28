using Medium.Api.Infrastructure.Settings;

using Microsoft.Extensions.Options;

namespace Medium.Api.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
  public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<AppSettings>(configuration);
    return services;
  }

  public static AppSettings GetAppSettings(this IServiceCollection services)
  {
    var provider = services.BuildServiceProvider();
    return provider.GetRequiredService<IOptions<AppSettings>>().Value;
  }
}