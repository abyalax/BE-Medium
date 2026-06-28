using System.Text.Json;

using Medium.Api.Infrastructure.Cache.Config;

using StackExchange.Redis;

namespace Medium.Api.Infrastructure.Cache.Services;

public sealed class RedisService(
    IRedisConnectionProvider connectionProvider,
    RedisConfiguration configuration)
{
  private readonly IRedisConnectionProvider _connectionProvider = connectionProvider;
  private readonly RedisConfiguration _configuration = configuration;

  private IDatabase GetDatabase() => _connectionProvider.Connection.GetDatabase(_configuration.Database);

  public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    var value = await db.StringGetAsync(key);
    return value.IsNull ? null : value.ToString();
  }

  public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
  {
    var value = await GetAsync(key, cancellationToken);
    return value is null ? default : JsonSerializer.Deserialize<T>(value);
  }

  public async Task<bool> SetAsync(
      string key,
      string value,
      TimeSpan? expiry = null,
      CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    if (expiry.HasValue)
    {
      return await db.StringSetAsync(key, value, expiry.Value);
    }
    return await db.StringSetAsync(key, value);
  }

  public async Task<bool> SetAsync<T>(
      string key,
      T value,
      TimeSpan? expiry = null,
      CancellationToken cancellationToken = default)
  {
    var json = JsonSerializer.Serialize(value);
    return await SetAsync(key, json, expiry, cancellationToken);
  }

  public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    return await db.KeyDeleteAsync(key);
  }

  public async Task<bool> DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
    return await db.KeyDeleteAsync(redisKeys) > 0;
  }

  public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    return await db.KeyExistsAsync(key);
  }

  public async Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    return await db.StringIncrementAsync(key);
  }

  public async Task<long> DecrementAsync(string key, CancellationToken cancellationToken = default)
  {
    var db = GetDatabase();
    return await db.StringDecrementAsync(key);
  }

  public async Task<bool> SetExAsync(
      string key,
      string value,
      int secondsExpiry,
      CancellationToken cancellationToken = default)
  {
    var expiry = TimeSpan.FromSeconds(secondsExpiry);
    return await SetAsync(key, value, expiry, cancellationToken);
  }
}