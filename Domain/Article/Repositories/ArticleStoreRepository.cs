using Medium.Api.Infrastructure.Database;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Article.Repositories;

public class ArticleStoreRepository(ApplicationDbContext context)
{

  public async Task AddAsync(ArticleModel article, CancellationToken cancellationToken = default)
  {
    await context.Articles.AddAsync(article, cancellationToken);
  }

  public async Task<ArticleModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Articles.FindAsync(new object[] { id }, cancellationToken);
  }

  public void Remove(ArticleModel article)
  {
    context.Articles.Remove(article);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

  public async Task ReplaceArticleContentImagesAsync(Guid articleId, IReadOnlyCollection<Guid> contentImageIds, CancellationToken cancellationToken = default)
  {
    var existingImages = await context.ObjectStorages
      .Where(os => os.ArticleId == articleId)
      .ToListAsync(cancellationToken);

    foreach (var image in existingImages)
    {
      image.ArticleId = null;
    }

    if (contentImageIds.Count > 0)
    {
      var images = await context.ObjectStorages
        .Where(os => contentImageIds.Contains(os.Id))
        .ToListAsync(cancellationToken);

      foreach (var image in images)
      {
        image.ArticleId = articleId;
      }
    }
  }

  public async Task ReplaceArticleTagsAsync(Guid articleId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
  {
    var existingTags = await context.ArticleTags
      .Where(at => at.ArticleId == articleId)
      .ToListAsync(cancellationToken);

    context.ArticleTags.RemoveRange(existingTags);

    foreach (var tagId in tagIds)
    {
      await context.ArticleTags.AddAsync(new ArticleTag
      {
        ArticleId = articleId,
        TagId = tagId
      }, cancellationToken);
    }
  }

}