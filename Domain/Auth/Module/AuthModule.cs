using Medium.Api.Domain.Auth.Commands.Handlers;
using Medium.Api.Domain.Auth.EventHandlers;
using Medium.Api.Domain.Auth.Queries.Handlers;
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

    services.AddScoped<LoginHandler>();
    services.AddScoped<RegisterHandler>();
    services.AddScoped<AssignPermissionToRoleHandler>();
    services.AddScoped<AssignRoleToUserHandler>();
    services.AddScoped<CreateRoleHandler>();
    services.AddScoped<DeleteRoleHandler>();
    services.AddScoped<UpdateRoleHandler>();
    services.AddScoped<GetAllPermissionsHandler>();
    services.AddScoped<GetAllRolesHandler>();
    services.AddScoped<GetPermissionByIdHandler>();
    services.AddScoped<GetPermissionsByRoleHandler>();
    services.AddScoped<GetPermissionsByUserHandler>();
    services.AddScoped<GetRoleByIdHandler>();
    services.AddScoped<GetUserByIdHandler>();

    return services;
  }
}