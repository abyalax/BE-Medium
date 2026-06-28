using Minio.DataModel;
using Minio.DataModel.Args;

namespace Medium.Api.Infrastructure.Storage;

public interface IMinioClient : IAsyncDisposable
{
  Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
  Task MakeBucketAsync(string bucketName, CancellationToken cancellationToken = default);
  Task PutObjectAsync(string bucketName, string objectName, Stream data, long size, string contentType, CancellationToken cancellationToken = default);
  Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
  Task RemoveObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
  Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt, CancellationToken cancellationToken = default);
}