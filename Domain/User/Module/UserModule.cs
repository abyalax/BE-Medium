using FluentValidation;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Domain.User.Services;
using static Medium.Api.Http.Api.Version1.Users.UserEndpoints;

namespace Medium.Api.Domain.User.Module;

public static class UserModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<UserRepository>();

        services.AddScoped<UserService>();

        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
        services.AddScoped<IValidator<AssignUserRolesRequest>, AssignUserRolesRequestValidator>();

        return services;
    }
}