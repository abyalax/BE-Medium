using Medium.Api.Infrastructure.Interface;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsConnectionProvider
{
  NatsConnection Connection { get; }
}

public class NatsConnectionProvider : INatsConnectionProvider, INatsLifecycle
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<NatsConnectionProvider> _logger;
  private NatsConnection? _connection;

  public NatsConnectionProvider(IConfiguration configuration, ILogger<NatsConnectionProvider> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  public NatsConnection Connection => _connection ?? throw new InvalidOperationException("NATS is not connected. Call InitializeAsync first.");

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var url = _configuration["Nats:Url"] ?? "nats://localhost:4222";
    _logger.LogInformation("Connecting to NATS at {Url}...", url);

    var opts = NatsOpts.Default with { Url = url };
    _connection = new NatsConnection(opts);

    // Ensure connection is established
    await _connection.ConnectAsync();

    _logger.LogInformation("✓ Connected to NATS");
  }

  public async Task ShutdownAsync(CancellationToken cancellationToken = default)
  {
    if (_connection != null)
    {
      _logger.LogInformation("Disconnecting from NATS...");
      await _connection.DisposeAsync();
      _connection = null;
      _logger.LogInformation("✗ Disconnected from NATS");
    }
  }
}