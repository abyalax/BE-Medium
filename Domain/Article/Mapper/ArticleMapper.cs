using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Storage.Mapper;
using Medium.Api.Domain.Tag.Dtos;

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

    var contentImages = article.ContentImages.Select(ObjectStorageMapper.ToResponse).ToList();

    var thumbnail = article.Thumbnail == null ? null : ObjectStorageMapper.ToResponse(article.Thumbnail);

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