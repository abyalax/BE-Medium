using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Storage.Dtos;
using Medium.Api.Domain.User.Mapper;

using ObjectStorageModel = Medium.Api.Models.ObjectStorage;

namespace Medium.Api.Domain.Storage.Mapper;

public class ObjectStorageMapper
{
  public static ObjectStorageDto ToResponse(ObjectStorageModel objectStorage)
  {
    // Map Article if available
    var articleDto = objectStorage.Article == null
      ? null
      : ArticleMapper.ToResponse(objectStorage.Article);

    // Map Author if available
    var authorDto = objectStorage.Author == null
      ? null
      : UserMapper.ToResponse(objectStorage.Author);

    return new ObjectStorageDto(
      objectStorage.AuthorId,
      objectStorage.ArticleId,
      objectStorage.Bucket,
      objectStorage.ObjectKey,
      objectStorage.MimeType,
      objectStorage.OriginalName,
      objectStorage.Size,
      objectStorage.AccessTypes,
      authorDto,
      articleDto
    );
  }
}