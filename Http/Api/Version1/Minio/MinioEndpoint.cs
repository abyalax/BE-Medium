
using Medium.Api.Infrastructure.Minio.Services;

namespace Medium.Api.Http.Api.Version1.Minio;

public static class MinioEndpoints
{
  public static void MapMinioEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/storage")
      .WithTags("Storage")
      .RequireAuthorization();

    group.MapPost("/upload", async (
        MinioService minioService,
        IFormFile file,
        CancellationToken cancellationToken
      ) =>
    {
      var objectName = $"{Guid.NewGuid()}-{file.FileName}";
      await minioService.UploadAsync(objectName, file.OpenReadStream(), file.ContentType, cancellationToken);

      return Results.Ok(new { objectName, message = "File uploaded successfully" });
    })
    .WithName("UploadFile")
    .WithOpenApi()
    .Accepts<IFormFile>("multipart/form-data");

    group.MapGet("/download/{objectName}", async (
      string objectName,
      MinioService minioService,
      CancellationToken cancellationToken) =>
    {
      var stream = await minioService.DownloadAsync(objectName, cancellationToken);
      return Results.File(stream, "application/octet-stream", objectName);
    })
    .WithName("DownloadFile")
    .WithOpenApi();

    group.MapGet("/url/{objectName}", async (
      string objectName,
      MinioService minioService,
      CancellationToken cancellationToken) =>
    {
      var url = await minioService.GetPresignedUrlAsync(objectName, 3600, cancellationToken);
      return Results.Ok(new { url });
    })
    .WithName("GetPresignedUrl")
    .WithOpenApi();

    group.MapDelete("/{objectName}", async (
      string objectName,
      MinioService minioService,
      CancellationToken cancellationToken) =>
    {
      await minioService.DeleteAsync(objectName, cancellationToken);
      return Results.Ok(new { message = "File deleted successfully" });
    })
    .WithName("DeleteFile")
    .WithOpenApi();
  }
}