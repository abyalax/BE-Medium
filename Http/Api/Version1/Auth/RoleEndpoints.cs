using Microsoft.AspNetCore.Mvc;
using Medium.Api.Domain.Auth.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

namespace Medium.Api.Http.Api.Version1.Auth;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .AddEndpointFilter<ValidationEndpointFilter>();

        group.MapPost("/", async (
            [FromBody] CreateRoleRequest request,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            var response = await roleService.CreateRoleAsync(request.Name, request.Description, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(response, "Created"), statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization(Permissions.Roles.Create)
        .WithName("CreateRole")
        .WithOpenApi();

        group.MapGet("/", async (
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            var roles = await roleService.GetAllRolesAsync(cancellationToken);
            return Results.Json(ApiResponseWriter.Success(roles));
        })
        .RequireAuthorization(Permissions.Roles.Get)
        .WithName("GetAllRoles")
        .WithOpenApi();

        group.MapGet("/{roleId:guid}", async (
            Guid roleId,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            return Results.Json(ApiResponseWriter.Success(await roleService.GetRoleByIdAsync(roleId, cancellationToken)));
        })
        .RequireAuthorization(Permissions.Roles.Get)
        .WithName("GetRoleById")
        .WithOpenApi();

        group.MapPut("/{roleId:guid}", async (
            Guid roleId,
            [FromBody] CreateRoleRequest request,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            return Results.Json(ApiResponseWriter.Success(await roleService.UpdateRoleAsync(roleId, request.Name, request.Description, cancellationToken)));
        })
        .RequireAuthorization(Permissions.Roles.Update)
        .WithName("UpdateRole")
        .WithOpenApi();

        group.MapDelete("/{roleId:guid}", async (
            Guid roleId,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            await roleService.DeleteRoleAsync(roleId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
        })
        .RequireAuthorization(Permissions.Roles.Delete)
        .WithName("DeleteRole")
        .WithOpenApi();

        group.MapPost("/{roleId:guid}/permissions/{permissionId:guid}", async (
            Guid roleId,
            Guid permissionId,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            await roleService.AssignPermissionToRoleAsync(roleId, permissionId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Assigned"));
        })
        .RequireAuthorization(Permissions.Roles.AssignPermissions)
        .WithName("AssignPermissionToRole")
        .WithOpenApi();

        group.MapPost("/users/{userId:guid}/roles/{roleId:guid}", async (
            Guid userId,
            Guid roleId,
            RoleService roleService,
            CancellationToken cancellationToken) =>
        {
            await roleService.AssignRoleToUserAsync(userId, roleId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Assigned"));
        })
        .RequireAuthorization(Permissions.Roles.AssignUsers)
        .WithName("AssignRoleToUser")
        .WithOpenApi();
    }

    public record CreateRoleRequest(string Name, string Description);
}
