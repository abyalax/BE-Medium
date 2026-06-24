using FluentValidation;
using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Follow.Services;

namespace Medium.Api.Domain.Follow.Module;

public static class FollowModule
{
    public static IServiceCollection AddFollowModule(this IServiceCollection services)
    {
        services.AddScoped<FollowRepository>();

        services.AddScoped<FollowService>();

        services.AddScoped<IValidator<FollowRequest>, FollowRequestValidator>();

        return services;
    }
}