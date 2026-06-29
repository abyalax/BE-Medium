using Medium.Api.Infrastructure.Cache.Config;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Interface;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Medium.Api.Infrastructure.Cache.Module;

public static class CacheModule
{
  public static IServiceCollection AddRedisInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var redisConfig = configuration
       .GetSection("Redis")
       .Get<RedisConfiguration>()
       ?? throw new InvalidOperationException("Redis configuration is missing.");

    services.AddSingleton(redisConfig);
    services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();
    services.AddSingleton<IInfrastructureLifecycle>(sp => (RedisConnectionProvider)sp.GetRequiredService<IRedisConnectionProvider>());
    services.AddSingleton<ICacheLifecycle>(sp => (RedisConnectionProvider)sp.GetRequiredService<IRedisConnectionProvider>());
    services.AddSingleton<IConnectionMultiplexer>(sp => sp.GetRequiredService<IRedisConnectionProvider>().Connection);
    services.AddScoped<RedisService>();

    return services;
  }
}