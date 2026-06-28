using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Consumers;
using Medium.Api.Infrastructure.Nats.Hosted;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Module;

public static class NatsModule
{
  public static IServiceCollection AddNatsInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    services.AddSingleton<NatsConnectionProvider>();
    services.AddSingleton<INatsConnectionProvider>(sp => sp.GetRequiredService<NatsConnectionProvider>());
    services.AddSingleton<INatsLifecycle>(sp => sp.GetRequiredService<NatsConnectionProvider>());
    services.AddSingleton<INatsConnection>(sp => sp.GetRequiredService<NatsConnectionProvider>().Connection);

    services.AddScoped<INatsPublisher, NatsPublisher>();
    services.AddScoped<IJetStreamEventPublisher, JetStreamEventPublisher>();
    services.AddScoped<INatsSubscriber, NatsSubscriber>();

    // TODO: re-enable when JetStream consumer API is aligned with NATS.Client.JetStream 2.x
    // services.AddHostedService<UserRegisteredPushConsumer>();
    // services.AddHostedService<UserLoggedInPullConsumer>();
    services.AddHostedService<EmailServiceResponder>();

    // Keep existing hosted service for backward compatibility
    services.AddHostedService<NatsSubscriptionHostedService>();

    return services;
  }
}