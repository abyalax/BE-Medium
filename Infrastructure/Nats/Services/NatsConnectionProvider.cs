using Medium.Api.Infrastructure.Interface;

using NATS.Client.Core;
using NATS.Client.Serializers.Json;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsConnectionProvider : INatsLifecycle
{
  NatsConnection Connection { get; }
}

public class NatsConnectionProvider(IConfiguration configuration, ILogger<NatsConnectionProvider> logger) : INatsConnectionProvider
{
  private NatsConnection? _connection;

  public NatsConnection Connection => _connection ?? throw new InvalidOperationException("NATS connection has not been initialized.");

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var url = configuration["AppSettings:Nats:Url"] ?? throw new InvalidOperationException("Nats URL is required");
    logger.LogInformation("Connecting to NATS at {Url}...", url);

    var opts = NatsOpts.Default with
    {
      Url = url,
      SerializerRegistry = NatsJsonSerializerRegistry.Default
    };
    _connection = new NatsConnection(opts);

    await _connection.ConnectAsync();
    logger.LogInformation("✓ Connected to NATS");
  }

  public async Task ShutdownAsync(CancellationToken cancellationToken = default)
  {
    if (_connection != null)
    {
      logger.LogInformation("Disconnecting from NATS...");
      await _connection.DisposeAsync();
      _connection = null;
      logger.LogInformation("✗ Disconnected from NATS");
    }
  }
}