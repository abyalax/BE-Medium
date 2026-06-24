using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Article;

public static class ArticleEndpoints
{
  public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/articles")
        .WithTags("Articles")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapGet("/", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] Guid? authorId,
        [FromQuery] string? tagSlug,
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var articleStatus = status != null ? System.Enum.Parse<Enums.ArticleStatus>(status, true) : (Enums.ArticleStatus?)null;
      var articles = await articleService.ListAsync(page, pageSize, authorId, tagSlug, searchTerm, articleStatus, sortBy, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(articles));
    })
    .RequireAuthorization(Permissions.Articles.Get)
    .WithName("ListArticles")
    .WithOpenApi();

    group.MapGet("/published", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var articles = await articleService.GetPublishedAsync(page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(articles));
    })
    .AllowAnonymous()
    .WithName("GetPublishedArticles")
    .WithOpenApi();

    group.MapGet("/search", async (
        [FromQuery] string searchTerm,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var articles = await articleService.SearchAsync(searchTerm, page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(articles));
    })
    .AllowAnonymous()
    .WithName("SearchArticles")
    .WithOpenApi();

    group.MapGet("/popular", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var articles = await articleService.GetPopularAsync(page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(articles));
    })
    .AllowAnonymous()
    .WithName("GetPopularArticles")
    .WithOpenApi();

    group.MapGet("/trending", async (
        [FromQuery] int limit,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var articles = await articleService.GetTrendingAsync(limit, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(articles));
    })
    .AllowAnonymous()
    .WithName("GetTrendingArticles")
    .WithOpenApi();

    group.MapGet("/{id:guid}", async (
        Guid id,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var article = await articleService.GetByIdAsync(id, cancellationToken);
      await articleService.IncrementViewCountAsync(id, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(article));
    })
    .RequireAuthorization(Permissions.Articles.Get)
    .WithName("GetArticleById")
    .WithOpenApi();

    group.MapGet("/slug/{slug}", async (
        string slug,
        ArticleService articleService,
        CancellationToken cancellationToken) =>
    {
      var article = await articleService.GetBySlugAsync(slug, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(article));
    })
    .AllowAnonymous()
    .WithName("GetArticleBySlug")
    .WithOpenApi();

    group.MapPost("/", async (
        [FromBody] CreateArticleRequest request,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var httpUser = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      var userId = Guid.Parse(httpUser!);

      return Results.Json(
              ApiResponseWriter.Success(
                  await articleService.CreateAsync(userId, request, cancellationToken),
                  "Created"
              ),
              statusCode: StatusCodes.Status201Created
          );
    })
    .RequireAuthorization(Permissions.Articles.Create)
    .WithName("CreateArticle")
    .WithOpenApi();

    group.MapPut("/{id:guid}", async (
        Guid id,
        [FromBody] UpdateArticleRequest request,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      return Results.Json(ApiResponseWriter.Success(
              await articleService.UpdateAsync(id, userId, isAdmin, request, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Articles.UpdateOwn)
    .WithName("UpdateArticle")
    .WithOpenApi();

    group.MapPost("/{id:guid}/publish", async (
        Guid id,
        [FromBody] PublishArticleRequest request,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      return Results.Json(ApiResponseWriter.Success(
              await articleService.PublishAsync(id, userId, isAdmin, request, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Articles.Publish)
    .WithName("PublishArticle")
    .WithOpenApi();

    group.MapPost("/{id:guid}/unpublish", async (
        Guid id,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      return Results.Json(ApiResponseWriter.Success(
              await articleService.UnPublishAsync(id, userId, isAdmin, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Articles.Publish)
    .WithName("UnPublishArticle")
    .WithOpenApi();

    group.MapPost("/{id:guid}/archive", async (
        Guid id,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      return Results.Json(ApiResponseWriter.Success(
              await articleService.ArchiveAsync(id, userId, isAdmin, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Articles.Archive)
    .WithName("ArchiveArticle")
    .WithOpenApi();

    group.MapDelete("/{id:guid}", async (
        Guid id,
        ArticleService articleService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var isAdmin = httpContext.User.IsInRole("Admin");

      await articleService.DeleteAsync(id, userId, isAdmin, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.Articles.DeleteOwn)
    .WithName("DeleteArticle")
    .WithOpenApi();
  }
}