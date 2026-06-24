using Medium.Api.Infrastructure.Nats.Hosted;
using Medium.Api.Infrastructure.Nats.Services;

using NATS.Client;

namespace Medium.Api.Infrastructure.Nats.Module;

public static class NatsModule
{
  public static IServiceCollection AddNatsInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var url = configuration["Nats:Url"];

    var opts = ConnectionFactory.GetDefaultOptions();
    opts.Url = url;

    var connection = new ConnectionFactory()
        .CreateConnection(opts);

    services.AddSingleton(connection);

    services.AddScoped<INatsPublisher, NatsPublisher>();
    services.AddScoped<INatsSubscriber, NatsSubscriber>();

    services.AddHostedService<NatsSubscriptionHostedService>();

    return services;
  }
}