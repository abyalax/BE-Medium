using Medium.Api.Domain.Storage.Dtos;
using Medium.Api.Domain.Storage.Mapper;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Storage.Config;
using Medium.Api.Infrastructure.Storage.Repositories;
using Medium.Api.Models;

using Minio;
using Minio.DataModel.Args;

namespace Medium.Api.Infrastructure.Storage.Services;

public sealed class StorageService(
    Minio.IMinioClient minioClient,
    MinioConfiguration configuration,
    ObjectStorageRepository objectStorageRepository)
{
  private readonly Minio.IMinioClient _minioClient = minioClient;
  private readonly MinioConfiguration _configuration = configuration;
  private readonly ObjectStorageRepository _objectStorageRepository = objectStorageRepository;

  public async Task<string> GetPresignedUploadUrlAsync(
      Guid authorId,
      int expirySeconds = 3600,
      CancellationToken cancellationToken = default)
  {
    var objectKey = $"{Guid.NewGuid()}";

    var uploadUrl = await _minioClient.PresignedPutObjectAsync(
      new PresignedPutObjectArgs()
        .WithBucket(_configuration.BucketName)
        .WithObject(objectKey)
        .WithExpiry(expirySeconds));

    var objectStorage = new ObjectStorage
    {
      Bucket = _configuration.BucketName,
      ObjectKey = objectKey,
      AuthorId = authorId,
      MimeType = "application/octet-stream",
      OriginalName = objectKey,
      Size = 0,
      AccessTypes = FileAccessType.Private
    };

    await _objectStorageRepository.AddAsync(objectStorage, cancellationToken);
    await _objectStorageRepository.SaveAsync(cancellationToken);

    return uploadUrl;
  }

  public async Task<string> GetPresignedDownloadUrlAsync(
    string objectKey,
    int expirySeconds = 3600,
    CancellationToken cancellationToken = default
  )
  {
    return await _minioClient.PresignedGetObjectAsync(
      new PresignedGetObjectArgs()
        .WithBucket(_configuration.BucketName)
        .WithObject(objectKey)
        .WithExpiry(expirySeconds));
  }

  public async Task<ObjectStorageDto?> GetMetadataAsync(
      string objectKey,
      CancellationToken cancellationToken = default)
  {
    var objectStorage = await _objectStorageRepository.GetByKeyAsync(objectKey, cancellationToken);

    if (objectStorage is null)
      return null;

    return ObjectStorageMapper.ToResponse(objectStorage);
  }

  public async Task DeleteAsync(
      string objectKey,
      CancellationToken cancellationToken = default)
  {
    var objectStorage = await _objectStorageRepository.GetByKeyAsync(objectKey, cancellationToken);

    if (objectStorage is not null)
    {
      _objectStorageRepository.Delete(objectStorage);
      await _objectStorageRepository.SaveAsync(cancellationToken);
    }

    await _minioClient.RemoveObjectAsync(
        new RemoveObjectArgs()
            .WithBucket(_configuration.BucketName)
            .WithObject(objectKey),
        cancellationToken);
  }
}