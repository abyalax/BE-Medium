using Microsoft.AspNetCore.Mvc;
using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Medium.Api.Http.Api.Version1.Bookmark;

public static class BookmarkEndpoints
{
    public static void MapBookmarkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookmarks")
            .WithTags("Bookmarks")
            .AddEndpointFilter<ValidationEndpointFilter>();

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            BookmarkService bookmarkService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var bookmarks = await bookmarkService.GetByUserAsync(userId, page, pageSize, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(bookmarks));
        })
        .RequireAuthorization(Permissions.Bookmarks.Create)
        .WithName("GetUserBookmarks")
        .WithOpenApi();

        group.MapGet("/{id:guid}", async (
            Guid id,
            BookmarkService bookmarkService,
            CancellationToken cancellationToken) =>
        {
            return Results.Json(ApiResponseWriter.Success(await bookmarkService.GetByIdAsync(id, cancellationToken)));
        })
        .RequireAuthorization(Permissions.Bookmarks.Create)
        .WithName("GetBookmarkById")
        .WithOpenApi();

        group.MapPost("/", async (
            [FromBody] BookmarkRequest request,
            BookmarkService bookmarkService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            return Results.Json(ApiResponseWriter.Success(
                await bookmarkService.CreateAsync(userId, request, cancellationToken),
                "Created"),
                statusCode: StatusCodes.Status201Created);
        })
        .RequireAuthorization(Permissions.Bookmarks.Create)
        .WithName("CreateBookmark")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (
            Guid id,
            BookmarkService bookmarkService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            await bookmarkService.DeleteAsync(id, userId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
        })
        .RequireAuthorization(Permissions.Bookmarks.Create)
        .WithName("DeleteBookmark")
        .WithOpenApi();

        group.MapDelete("/article/{articleId:guid}", async (
            Guid articleId,
            BookmarkService bookmarkService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            await bookmarkService.RemoveByArticleAsync(userId, articleId, cancellationToken);
            return Results.Json(ApiResponseWriter.Success(null, "Removed"));
        })
        .RequireAuthorization(Permissions.Bookmarks.Create)
        .WithName("RemoveBookmarkByArticle")
        .WithOpenApi();
    }
}
