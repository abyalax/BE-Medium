using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Domain.Auth.Services;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

namespace Medium.Api.Http.Api.Version1.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .AddEndpointFilter<ValidationEndpointFilter>();

        group.MapPost("/register", async (
            RegisterRequest request,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(response, "Created"), statusCode: StatusCodes.Status201Created);
        })
        .WithName("Register")
        .WithOpenApi();

        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            var response = await authService.LoginAsync(request, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(response));
        })
        .WithName("Login")
        .WithOpenApi();
    }
}
