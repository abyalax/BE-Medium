using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Services;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Tag;

public static class TagEndpoints
{
  public static void MapTagEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/tags")
        .WithTags("Tags")
        .AddEndpointFilter<ValidationEndpointFilter>();

    group.MapGet("/", async (
        [FromQuery] int page,
        [FromQuery] int pageSize,
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      var tags = await tagService.ListAsync(page, pageSize, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(tags));
    })
    .RequireAuthorization(Permissions.Tags.Manage)
    .WithName("ListTags")
    .WithOpenApi();

    group.MapGet("/all", async (
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      var tags = await tagService.GetAllAsync(cancellationToken);
      return Results.Json(ApiResponseWriter.Success(tags));
    })
    .AllowAnonymous()
    .WithName("GetAllTags")
    .WithOpenApi();

    group.MapGet("/{id:guid}", async (
        Guid id,
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await tagService.GetByIdAsync(id, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Tags.Manage)
    .WithName("GetTagById")
    .WithOpenApi();

    group.MapPost("/", async (
        [FromBody] CreateTagRequest request,
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await tagService.CreateAsync(request, cancellationToken), "Created"), statusCode: StatusCodes.Status201Created);
    })
    .RequireAuthorization(Permissions.Tags.Manage)
    .WithName("CreateTag")
    .WithOpenApi();

    group.MapPut("/{id:guid}", async (
        Guid id,
        [FromBody] UpdateTagRequest request,
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      return Results.Json(ApiResponseWriter.Success(await tagService.UpdateAsync(id, request, cancellationToken)));
    })
    .RequireAuthorization(Permissions.Tags.Manage)
    .WithName("UpdateTag")
    .WithOpenApi();

    group.MapDelete("/{id:guid}", async (
        Guid id,
        TagService tagService,
        CancellationToken cancellationToken) =>
    {
      await tagService.DeleteAsync(id, cancellationToken);
      return Results.Json(ApiResponseWriter.Success(null, "Deleted"));
    })
    .RequireAuthorization(Permissions.Tags.Manage)
    .WithName("DeleteTag")
    .WithOpenApi();
  }
}