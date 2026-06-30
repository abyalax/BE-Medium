
using Medium.Api.Infrastructure.Settings.Dtos;
using Medium.Api.Infrastructure.Settings.Validation;

namespace Medium.Api.Infrastructure.Settings.Module;

public static class SettingsModule
{
  public static IServiceCollection AddSettingsModule(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<ApplicationSettings>()
        .Bind(configuration.GetSection("AppSettings"))
        .Validate(settings =>
        {
          var validator = new ApplicationSettingsValidation();
          var validationResult = validator.Validate(settings);

          if (!validationResult.IsValid)
          {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Configuration validation failed: {errors}");
          }

          return true;
        })
        .ValidateOnStart();

    return services;
  }
}