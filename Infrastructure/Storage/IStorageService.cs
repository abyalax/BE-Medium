namespace Medium.Api.Infrastructure.Storage;

public interface IStorageService
{
  Task<string> UploadFileAsync(Stream stream, string bucketName, string objectName, string contentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
  Task<Stream> DownloadFileAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
  Task DeleteFileAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
  Task<string> GeneratePresignedUrlAsync(string bucketName, string objectName, int expiresIn = 3600, CancellationToken cancellationToken = default);
  Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
}