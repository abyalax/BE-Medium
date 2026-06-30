using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Infrastructure.Storage.Dtos;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Article.Mapper;

public class ArticleMapper
{
  public static ArticleDto ToResponse(ArticleModel article)
  {
    var tags = article.ArticleTags.Select(at => new TagDto(
      at.Tag.Id,
      at.Tag.Name,
      at.Tag.Slug
    )).ToList();

    var contentImages = article.ContentImages.Select(os => new ObjectStorageDto(
      os.Id,
      os.ObjectKey,
      os.Bucket,
      os.MimeType,
      os.OriginalName,
      os.Size,
      os.AccessTypes,
      null,
      os.CreatedAt
    )).ToList();

    var thumbnail = article.Thumbnail == null ? null : new ObjectStorageDto(
      article.Thumbnail.Id,
      article.Thumbnail.Bucket,
      article.Thumbnail.ObjectKey,
      article.Thumbnail.MimeType,
      article.Thumbnail.OriginalName,
      article.Thumbnail.Size ?? 0,
      article.Thumbnail.AccessTypes,
      null,
      article.Thumbnail.CreatedAt
    );

    return new ArticleDto(
      article.Id,
      article.AuthorId,
      article.Author.Name,
      article.Title,
      article.Slug,
      article.Content,
      article.Summary,
      article.CoverImageUrl,
      article.ThumbnailId,
      thumbnail,
      contentImages,
      article.Status,
      article.PublishedAt,
      article.ScheduledAt,
      article.ViewCount,
      tags,
      article.CreatedAt,
      article.UpdatedAt
    );
  }
}