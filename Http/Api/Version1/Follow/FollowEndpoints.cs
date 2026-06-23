using Microsoft.AspNetCore.Mvc;
using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Medium.Api.Http.Api.Version1.Follow;

public static class FollowEndpoints
{
    public static void MapFollowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/follows")
            .WithTags("Follows")
            .AddEndpointFilter<ValidationEndpointFilter>();

        group.MapGet("/followers/{userId:guid}", async (
            Guid userId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            FollowService followService,
            CancellationToken cancellationToken) =>
        {
            var followers = await followService.GetFollowersAsync(userId, page, pageSize, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(followers));
        })
        .RequireAuthorization(Permissions.Authors.Follow)
        .WithName("GetUserFollowers")
        .WithOpenApi();

        group.MapGet("/following/{userId:guid}", async (
            Guid userId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            FollowService followService,
            CancellationToken cancellationToken) =>
        {
            var following = await followService.GetFollowingAsync(userId, page, pageSize, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(following));
        })
        .RequireAuthorization(Permissions.Authors.Follow)
        .WithName("GetUserFollowing")
        .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] FollowRequest request,
            FollowService followService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var followerId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            return Results.Json(ApiResponseWriter.Success(
                await followService.CreateAsync(followerId, request, cancellationToken),
                "Created"),
                statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization(Permissions.Authors.Follow)
        .WithName("FollowUser")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (
            Guid id,
            FollowService followService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            await followService.DeleteAsync(id, currentUserId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Unfollowed"));
        })
        .RequireAuthorization(Permissions.Authors.Follow)
        .WithName("UnfollowUser")
        .WithOpenApi();

        group.MapDelete("/by-user/{followingId:guid}", async (
            Guid followingId,
            FollowService followService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var followerId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            await followService.UnfollowAsync(followerId, followingId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Unfollowed"));
        })
        .RequireAuthorization(Permissions.Authors.Follow)
        .WithName("UnfollowUserByUserId")
        .WithOpenApi();
    }
}
