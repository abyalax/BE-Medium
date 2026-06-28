namespace Medium.Api.Infrastructure.Cache.Config;

public sealed class RedisConfiguration
{
  public string Host { get; init; } = string.Empty;

  public int Port { get; init; }

  public int Database { get; init; }

  public string? Password { get; init; }

  public int ConnectionTimeout { get; init; } = 5000;

  public int SyncTimeout { get; init; } = 5000;
}