using Medium.Api.Domain.Follow.Repositories;
namespace Medium.Api.Domain.Follow.Module;

public static class FollowModule
{
  public static IServiceCollection AddFollowModule(this IServiceCollection services)
  {
    services.AddScoped<FollowQueryRepository>();
    services.AddScoped<FollowStoreRepository>();

    return services;
  }
}