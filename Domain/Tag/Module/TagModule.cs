using Medium.Api.Domain.Tag.Repositories;
namespace Medium.Api.Domain.Tag.Module;

public static class TagModule
{
  public static IServiceCollection AddTagModule(this IServiceCollection services)
  {
    services.AddScoped<TagQueryRepository>();
    services.AddScoped<TagStoreRepository>();

    return services;
  }
}