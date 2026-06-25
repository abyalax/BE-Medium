using Medium.Api.Infrastructure.Email.Config;
using Medium.Api.Infrastructure.Email.Services;

namespace Medium.Api.Infrastructure.Email.Module;

public static class EmailModule
{
  public static IServiceCollection AddEmailInfrastructure(
  this IServiceCollection services,
  IConfiguration configuration)
  {
    var emailConfiguration = configuration
      .GetSection("Email")
      .Get<EmailConfiguration>()
      ?? throw new InvalidOperationException("Email configuration is missing.");

    services.AddSingleton(emailConfiguration);
    services.AddSingleton<EmailTemplateService>();
    services.AddScoped<MailpitEmailService>();

    return services;
  }
}