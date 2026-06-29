using Medium.Api.Infrastructure.Settings;

using Microsoft.Extensions.Options;

using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace Medium.Api.Infrastructure.Storage;

public class MinioClientWrapper : IMinioClient
{
  private readonly Minio.IMinioClient _client;

  public MinioClientWrapper(IOptions<AppSettings> settings)
  {
    var minioSettings = settings.Value.Minio;
    _client = new MinioClient()
        .WithEndpoint(minioSettings.Endpoint)
        .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
        .WithSSL(minioSettings.UseSsl)
        .WithRegion(minioSettings.Region)
        .Build();
  }

  public async Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
  {
    var args = new BucketExistsArgs()
        .WithBucket(bucketName);
    return await _client.BucketExistsAsync(args, cancellationToken);
  }

  public async Task MakeBucketAsync(string bucketName, CancellationToken cancellationToken = default)
  {
    var args = new MakeBucketArgs()
        .WithBucket(bucketName);
    await _client.MakeBucketAsync(args, cancellationToken);
  }

  public async Task PutObjectAsync(string bucketName, string objectName, Stream data, long size, string contentType, CancellationToken cancellationToken = default)
  {
    var args = new PutObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName)
        .WithStreamData(data)
        .WithObjectSize(size)
        .WithContentType(contentType);
    await _client.PutObjectAsync(args, cancellationToken);
  }

  public async Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
  {
    var memoryStream = new MemoryStream();
    var args = new GetObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName)
        .WithCallbackStream(stream =>
        {
          stream.CopyTo(memoryStream);
        });
    await _client.GetObjectAsync(args, cancellationToken);
    memoryStream.Position = 0;
    return memoryStream;
  }

  public async Task RemoveObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
  {
    var args = new RemoveObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName);
    await _client.RemoveObjectAsync(args, cancellationToken);
  }

  public async Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt, CancellationToken cancellationToken = default)
  {
    var args = new PresignedGetObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName)
        .WithExpiry(expiresInt);
    return await _client.PresignedGetObjectAsync(args);
  }

  public ValueTask DisposeAsync()
  {
    _client.Dispose();
    return ValueTask.CompletedTask;
  }
}