using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Users;

public static class UserEndpoints
{
  public static void MapUserEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/users")
        .WithTags("Users")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapGet("/me", async (
        HttpContext httpContext,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      return Results.Json(ApiResponseWriter.Success(await userService.GetByIdAsync(userId, cancellationToken)));
    })
    .RequireAuthorization()
    .WithName("GetCurrentUser")
    .WithOpenApi();

    group.MapGet("/", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      var users = await userService.ListAsync(page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(users));
    })
    .RequireAuthorization(Permissions.Users.Get)
    .WithName("ListUsers")
    .WithOpenApi();

    group.MapGet("/{id:guid}", async (
        Guid id,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await userService.GetByIdAsync(id, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Users.Get)
    .WithName("GetUserById")
    .WithOpenApi();

    group.MapGet("/by-email", async (
        [FromQuery] string email,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await userService.FindByEmailAsync(email, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Users.Get)
    .WithName("FindUserByEmail")
    .WithOpenApi();

    group.MapPost("/", async (
        [FromBody] CreateUserRequest request,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await userService.CreateAsync(request, cancellationToken), "Created"), statusCode: StatusCodes.Status201Created);
    })
    .RequireAuthorization(Permissions.Users.Create)
    .WithName("CreateUser")
    .WithOpenApi();

    group.MapPut("/{id:guid}", async (
        Guid id,
        [FromBody] UpdateUserRequest request,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await userService.UpdateAsync(id, request, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Users.Update)
    .WithName("UpdateUser")
    .WithOpenApi();

    group.MapPost("/{id:guid}/roles", async (
        Guid id,
        [FromBody] AssignUserRolesRequest request,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      await userService.AssignRolesAsync(id, request.RoleIds, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Assigned"));
    })
    .RequireAuthorization(Permissions.Users.AssignRoles)
    .WithName("AssignRolesToUser")
    .WithOpenApi();

    group.MapDelete("/{id:guid}", async (
        Guid id,
        UserService userService,
        CancellationToken cancellationToken) =>
    {
      await userService.DeleteAsync(id, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.Users.Delete)
    .WithName("DeleteUser")
    .WithOpenApi();
  }

  public record AssignUserRolesRequest(IReadOnlyCollection<Guid> RoleIds);
}