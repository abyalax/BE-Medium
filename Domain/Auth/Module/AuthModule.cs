using Medium.Api.Domain.Auth.EventHandlers;
using Medium.Api.Infrastructure.Auth;

using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Domain.Auth.Module;

public static class AuthModule
{
  public static IServiceCollection AddAuthModule(this IServiceCollection services)
  {
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

    services.AddScoped<OnUserRegisteredHandler>();
    services.AddScoped<OnUserLoggedInHandler>();

    return services;
  }
}