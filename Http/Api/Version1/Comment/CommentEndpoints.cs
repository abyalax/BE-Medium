using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Services;
using Medium.Api.Common.Constant;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Comment;

public static class CommentEndpoints
{
  public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/comments")
        .WithTags("Comments")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapGet("/{id:guid}", async (
        Guid id,
        CommentService commentService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await commentService.GetByIdAsync(id, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Comments.Create)
    .WithName("GetCommentById")
    .WithOpenApi();

    group.MapGet("/article/{articleId:guid}", async (
        Guid articleId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CommentService commentService,
        CancellationToken cancellationToken) =>
    {
      var comments = await commentService.GetByArticleAsync(articleId, page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(comments));
    })
    .AllowAnonymous()
    .WithName("GetCommentsByArticle")
    .WithOpenApi();

    group.MapGet("/user/{userId:guid}", async (
        Guid userId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CommentService commentService,
        CancellationToken cancellationToken) =>
    {
      var comments = await commentService.GetByUserAsync(userId, page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(comments));
    })
    .RequireAuthorization(Permissions.Comments.Create)
    .WithName("GetCommentsByUser")
    .WithOpenApi();

    group.MapPost("/", async (
        [FromBody] CreateCommentRequest request,
        CommentService commentService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

      return Results.Json(
              ApiResponseWriter.Success(
                  await commentService.CreateAsync(userId, request, cancellationToken),
                  "Created"
              ),
              statusCode: StatusCodes.Status201Created
          );
    })
    .RequireAuthorization(Permissions.Comments.Create)
    .WithName("CreateComment")
    .WithOpenApi();

    group.MapPut("/{id:guid}", async (
        Guid id,
        [FromBody] UpdateCommentRequest request,
        CommentService commentService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      return Results.Json(ApiResponseWriter.Success(
              await commentService.UpdateAsync(id, userId, isAdmin, request, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Comments.Create)
    .WithName("UpdateComment")
    .WithOpenApi();

    group.MapDelete("/{id:guid}", async (
        Guid id,
        CommentService commentService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      await commentService.DeleteAsync(id, userId, isAdmin, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.Comments.Create)
    .WithName("DeleteComment")
    .WithOpenApi();
  }
}