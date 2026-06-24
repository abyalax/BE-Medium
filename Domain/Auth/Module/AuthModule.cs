using Medium.Api.Domain.Auth.Services;
using Medium.Api.Http.Api.Version1.Auth;
using Medium.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Medium.Api.Domain.Auth.DTOs;
using FluentValidation;


namespace Medium.Api.Domain.Auth.Module;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RoleEndpoints.CreateRoleRequest>, CreateRoleRequestValidator>();
        services.AddScoped<IValidator<PermissionEndpoints.CreatePermissionRequest>, CreatePermissionRequestValidator>();

        services.AddScoped<AuthService>();
        services.AddScoped<RoleService>();
        services.AddScoped<PermissionService>();

        return services;
    }
}