namespace Medium.Api.Infrastructure.Storage;

public class S3StorageService : IStorageService
{
  private readonly IMinioClient _minioClient;

  public S3StorageService(IMinioClient minioClient)
  {
    _minioClient = minioClient;
  }

  public async Task<string> UploadFileAsync(Stream stream, string bucketName, string objectName, string contentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
  {
    await EnsureBucketExistsAsync(bucketName, cancellationToken);

    await _minioClient.PutObjectAsync(bucketName, objectName, stream, stream.Length, contentType, cancellationToken);
    return objectName;
  }

  public async Task<Stream> DownloadFileAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
  {
    return await _minioClient.GetObjectAsync(bucketName, objectName, cancellationToken);
  }

  public async Task DeleteFileAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
  {
    await _minioClient.RemoveObjectAsync(bucketName, objectName, cancellationToken);
  }

  public async Task<string> GeneratePresignedUrlAsync(string bucketName, string objectName, int expiresIn = 3600, CancellationToken cancellationToken = default)
  {
    return await _minioClient.PresignedGetObjectAsync(bucketName, objectName, expiresIn, cancellationToken);
  }

  public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
  {
    var exists = await _minioClient.BucketExistsAsync(bucketName, cancellationToken);
    if (!exists)
    {
      await _minioClient.MakeBucketAsync(bucketName, cancellationToken);
    }
  }
}