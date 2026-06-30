
using Medium.Api.Infrastructure.Settings.Dtos;

namespace Medium.Api.Infrastructure.Extensions;

public static class ConfigurationExtensions
{
  public static IServiceCollection ConfigureApplicationSettings(this IServiceCollection services, IConfiguration configuration)
  {
    return services.Configure<ApplicationSettings>(configuration);
  }
}