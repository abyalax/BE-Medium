using Medium.Api.Infrastructure.Cache.Config;
using Medium.Api.Infrastructure.Interface;

using StackExchange.Redis;

namespace Medium.Api.Infrastructure.Cache.Services;

public interface IRedisConnectionProvider
{
  IConnectionMultiplexer Connection { get; }
}

public class RedisConnectionProvider : IRedisConnectionProvider, ICacheLifecycle
{
  private readonly RedisConfiguration _config;
  private readonly ILogger<RedisConnectionProvider> _logger;
  private ConnectionMultiplexer? _connection;

  public RedisConnectionProvider(RedisConfiguration config, ILogger<RedisConnectionProvider> logger)
  {
    _config = config;
    _logger = logger;
  }

  public IConnectionMultiplexer Connection => _connection ?? throw new InvalidOperationException("Redis is not connected. Call InitializeAsync first.");

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Connecting to Redis at {Host}:{Port}...", _config.Host, _config.Port);

    var options = ConfigurationOptions.Parse($"{_config.Host}:{_config.Port}");
    options.DefaultDatabase = _config.Database;
    options.ConnectTimeout = _config.ConnectionTimeout;
    options.SyncTimeout = _config.SyncTimeout;

    if (!string.IsNullOrEmpty(_config.Password))
      options.Password = _config.Password;

    _connection = await ConnectionMultiplexer.ConnectAsync(options);
    _logger.LogInformation("✓ Connected to Redis");
  }

  public async Task ShutdownAsync(CancellationToken cancellationToken = default)
  {
    if (_connection != null)
    {
      _logger.LogInformation("Disconnecting from Redis...");
      await _connection.CloseAsync();
      _connection.Dispose();
      _connection = null;
      _logger.LogInformation("✗ Disconnected from Redis");
    }
  }
}