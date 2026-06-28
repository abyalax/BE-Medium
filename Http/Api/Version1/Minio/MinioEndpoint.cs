
using Medium.Api.Infrastructure.Storage.Services;

namespace Medium.Api.Http.Api.Version1.Minio;

public static class MinioEndpoints
{
  public static void MapMinioEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/storage")
        .WithTags("Storage")
        .RequireAuthorization();

    group.MapPost("/presigned-upload", async (
        StorageService StorageService,
        CancellationToken cancellationToken) =>
    {
      var authorId = Guid.NewGuid();
      var uploadUrl = await StorageService.GetPresignedUploadUrlAsync(authorId, 3600, cancellationToken);

      return Results.Ok(new { authorId, uploadUrl });
    })
    .WithName("GetPresignedUploadUrl")
    .WithOpenApi();

    group.MapPost("/presigned-download/{objectName}", async (
        string objectName,
        StorageService StorageService,
        CancellationToken cancellationToken) =>
    {
      var downloadUrl = await StorageService.GetPresignedDownloadUrlAsync(objectName, 3600, cancellationToken);
      return Results.Ok(new { downloadUrl });
    })
    .WithName("GetPresignedDownloadUrl")
    .WithOpenApi();

    group.MapDelete("/{objectName}", async (
        string objectName,
        StorageService StorageService,
        CancellationToken cancellationToken) =>
    {
      await StorageService.DeleteAsync(objectName, cancellationToken);
      return Results.Ok(new { message = "File deleted successfully" });
    })
    .WithName("DeleteFile")
    .WithOpenApi();
  }
}