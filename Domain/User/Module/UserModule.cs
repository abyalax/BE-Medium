using Medium.Api.Domain.User.Repositories;
namespace Medium.Api.Domain.User.Module;

public static class UserModule
{
  public static IServiceCollection AddUserModule(this IServiceCollection services)
  {
    services.AddScoped<UserQueryRepository>();
    services.AddScoped<UserStoreRepository>();
    services.AddScoped<RoleQueryRepository>();
    services.AddScoped<RoleStoreRepository>();
    services.AddScoped<PermissionQueryRepository>();
    return services;
  }
}