using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.ReadingHistory;

public static class ReadingHistoryEndpoints
{
  public static void MapReadingHistoryEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/reading-history")
        .WithTags("ReadingHistory")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapGet("/", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        ReadingHistoryService readingHistoryService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var history = await readingHistoryService.GetByUserAsync(userId, page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(history));
    })
    .RequireAuthorization(Permissions.ReadingHistory.Get)
    .WithName("GetUserReadingHistory")
    .WithOpenApi();

    group.MapGet("/recent", async (
        [FromQuery] int limit,
        ReadingHistoryService readingHistoryService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
      var history = await readingHistoryService.GetRecentByUserAsync(userId, limit, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(history));
    })
    .RequireAuthorization(Permissions.ReadingHistory.Get)
    .WithName("GetRecentReadingHistory")
    .WithOpenApi();

    group.MapGet("/{id:guid}", async (
        Guid id,
        ReadingHistoryService readingHistoryService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await readingHistoryService.GetByIdAsync(id, cancellationToken)));
    })
    .RequireAuthorization(Permissions.ReadingHistory.Get)
    .WithName("GetReadingHistoryById")
    .WithOpenApi();

    group.MapPost("/", async (
        [FromBody] CreateReadingHistoryRequest request,
        ReadingHistoryService readingHistoryService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

      return Results.Json(ApiResponseWriter.Success(
              await readingHistoryService.CreateAsync(userId, request, cancellationToken),
              "Created"),
              statusCode: StatusCodes.Status201Created);
    })
    .RequireAuthorization(Permissions.ReadingHistory.Get)
    .WithName("CreateReadingHistory")
    .WithOpenApi();

    group.MapDelete("/{id:guid}", async (
        Guid id,
        ReadingHistoryService readingHistoryService,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
    {
      var userId = Guid.Parse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

      await readingHistoryService.DeleteAsync(id, userId, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.ReadingHistory.Get)
    .WithName("DeleteReadingHistory")
    .WithOpenApi();
  }
}