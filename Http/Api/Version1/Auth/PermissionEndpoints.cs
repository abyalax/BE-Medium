using Medium.Api.Domain.Auth.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Auth;

public static class PermissionEndpoints
{
  public static void MapPermissionEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/permissions")
        .WithTags("Permissions")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapPost("/", async (
        [FromBody] CreatePermissionRequest request,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      var response = await permissionService.CreatePermissionAsync(
              request.Code,
              request.Name,
              request.Description,
              cancellationToken);

      return Results.Json(ApiResponseWriter.Success(response, "Created"), statusCode: StatusCodes.Status201Created);
    })
    .RequireAuthorization(Permissions.PermissionsModule.Create)
    .WithName("CreatePermission")
    .WithOpenApi();

    group.MapGet("/", async (
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      var permissions = await permissionService.GetAllPermissionsAsync(cancellationToken);
      return Results.Json(ApiResponseWriter.Success(permissions));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Get)
    .WithName("GetAllPermissions")
    .WithOpenApi();

    group.MapGet("/{permissionId:guid}", async (
        Guid permissionId,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await permissionService.GetPermissionByIdAsync(permissionId, cancellationToken)));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Get)
    .WithName("GetPermissionById")
    .WithOpenApi();

    group.MapPut("/{permissionId:guid}", async (
        Guid permissionId,
        [FromBody] CreatePermissionRequest request,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      var response = await permissionService.UpdatePermissionAsync(
              permissionId,
              request.Code,
              request.Name,
              request.Description,
              cancellationToken);

      return Results.Json(ApiResponseWriter.Success(response));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Update)
    .WithName("UpdatePermission")
    .WithOpenApi();

    group.MapDelete("/{permissionId:guid}", async (
        Guid permissionId,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      await permissionService.DeletePermissionAsync(permissionId, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Delete)
    .WithName("DeletePermission")
    .WithOpenApi();

    group.MapGet("/roles/{roleId:guid}", async (
        Guid roleId,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      var permissions = await permissionService.GetPermissionsByRoleAsync(roleId, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(permissions));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Get)
    .WithName("GetPermissionsByRole")
    .WithOpenApi();

    group.MapGet("/users/{userId:guid}", async (
        Guid userId,
        PermissionService permissionService,
        CancellationToken cancellationToken) =>
    {
      var permissions = await permissionService.GetPermissionsByUserAsync(userId, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(permissions));
    })
    .RequireAuthorization(Permissions.PermissionsModule.Get)
    .WithName("GetPermissionsByUser")
    .WithOpenApi();
  }

  public record CreatePermissionRequest(string Code, string Name, string Description);
}