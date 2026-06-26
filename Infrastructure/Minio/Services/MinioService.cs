

using Medium.Api.Infrastructure.Minio.Config;

using Minio;
using Minio.DataModel.Args;

namespace Medium.Api.Infrastructure.Minio.Services;

public sealed class MinioService(IMinioClient minioClient, MinioConfiguration configuration)
{
  private readonly IMinioClient _minioClient = minioClient;
  private readonly MinioConfiguration _configuration = configuration;

  public async Task<string> UploadAsync(
      string objectName,
      Stream stream,
      string contentType,
      CancellationToken cancellationToken = default
  )
  {
    await _minioClient.PutObjectAsync(
      new PutObjectArgs()
        .WithBucket(_configuration.BucketName)
        .WithObject(objectName)
        .WithStreamData(stream)
        .WithObjectSize(stream.Length)
        .WithContentType(contentType),
      cancellationToken);

    return objectName;
  }

  public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
  {
    await _minioClient.RemoveObjectAsync(
        new RemoveObjectArgs()
          .WithBucket(_configuration.BucketName)
          .WithObject(objectName),
        cancellationToken
      );
  }

  public async Task<Stream> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
  {
    var memoryStream = new MemoryStream();

    await _minioClient.GetObjectAsync(
        new GetObjectArgs()
          .WithBucket(_configuration.BucketName)
          .WithObject(objectName)
          .WithCallbackStream(stream => stream.CopyTo(memoryStream)),
        cancellationToken
      );

    memoryStream.Position = 0;
    return memoryStream;
  }

  public async Task<string> GetPresignedUrlAsync(string objectName, int expirySeconds = 3600, CancellationToken cancellationToken = default)
  {
    return await _minioClient.PresignedGetObjectAsync(
      new PresignedGetObjectArgs()
        .WithBucket(_configuration.BucketName)
        .WithObject(objectName)
        .WithExpiry(expirySeconds));
  }
}