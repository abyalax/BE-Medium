using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

using ArticleModel = Medium.Api.Models.Article;
using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Article.Repositories;

public class ArticleRepository
{
  private readonly ApplicationDbContext _context;

  public ArticleRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<ArticleModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
  }

  public async Task<ArticleModel?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
  }

  public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await _context.Articles.AnyAsync(a => a.Slug == slug, cancellationToken);
  }

  public async Task<bool> ExistsBySlugAsync(string slug, Guid excludeArticleId, CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .AnyAsync(a => a.Slug == slug && a.Id != excludeArticleId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> ListAsync(
      int page,
      int pageSize,
      Guid? authorId = null,
      string? tagSlug = null,
      string? searchTerm = null,
      Enums.ArticleStatus? status = null,
      string? sortBy = null,
      CancellationToken cancellationToken = default)
  {
    var query = _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .AsQueryable();

    if (authorId.HasValue)
    {
      query = query.Where(a => a.AuthorId == authorId.Value);
    }

    if (!string.IsNullOrEmpty(tagSlug))
    {
      query = query.Where(a => a.ArticleTags.Any(at => at.Tag.Slug == tagSlug));
    }

    if (!string.IsNullOrEmpty(searchTerm))
    {
      query = query.Where(a => a.Title.Contains(searchTerm) || a.Content.Contains(searchTerm));
    }

    if (status.HasValue)
    {
      query = query.Where(a => a.Status == status.Value);
    }

    query = sortBy?.ToLower() switch
    {
      "published" => query.OrderByDescending(a => a.PublishedAt),
      "views" => query.OrderByDescending(a => a.ViewCount),
      "created" => query.OrderByDescending(a => a.CreatedAt),
      _ => query.OrderByDescending(a => a.CreatedAt)
    };

    return await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> SearchPublishedAsync(
      string searchTerm,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .Where(a => a.Status == Enums.ArticleStatus.Published &&
                   (a.Title.Contains(searchTerm) || a.Content.Contains(searchTerm)))
        .OrderByDescending(a => a.PublishedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetPopularAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .Where(a => a.Status == Enums.ArticleStatus.Published)
        .OrderByDescending(a => a.ViewCount)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetTrendingAsync(
      int limit,
      CancellationToken cancellationToken = default)
  {
    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .Where(a => a.Status == Enums.ArticleStatus.Published &&
                   a.PublishedAt >= sevenDaysAgo)
        .OrderByDescending(a => a.ViewCount)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetPublishedAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .Where(a => a.Status == Enums.ArticleStatus.Published)
        .OrderByDescending(a => a.PublishedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetByAuthorAsync(
      Guid authorId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .Where(a => a.AuthorId == authorId)
        .OrderByDescending(a => a.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetScheduledAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Where(a => a.Status == Enums.ArticleStatus.Scheduled && a.ScheduledAt <= DateTime.UtcNow)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(cancellationToken);
  }

  public async Task<int> CountByAuthorAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(a => a.AuthorId == authorId, cancellationToken);
  }

  public async Task<int> CountPublishedAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(a => a.Status == Enums.ArticleStatus.Published, cancellationToken);
  }

  public async Task AddAsync(ArticleModel article, CancellationToken cancellationToken = default)
  {
    await _context.Articles.AddAsync(article, cancellationToken);
  }

  public void Remove(ArticleModel article)
  {
    _context.Articles.Remove(article);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task ReplaceArticleTagsAsync(Guid articleId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
  {
    var existingTags = await _context.ArticleTags
        .Where(at => at.ArticleId == articleId)
        .ToListAsync(cancellationToken);

    _context.ArticleTags.RemoveRange(existingTags);

    foreach (var tagId in tagIds)
    {
      await _context.ArticleTags.AddAsync(new ArticleTag
      {
        ArticleId = articleId,
        TagId = tagId
      }, cancellationToken);
    }
  }

  public async Task<IReadOnlyCollection<TagModel>> GetTagsByIdsAsync(IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
  {
    return await _context.Tags
        .Where(t => tagIds.Contains(t.Id))
        .ToListAsync(cancellationToken);
  }

  public async Task<TagModel?> GetTagBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await _context.Tags.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
  }

  public async Task<IReadOnlyCollection<TagModel>> GetAllTagsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Tags.ToListAsync(cancellationToken);
  }

  public async Task<ArticleWithAuthorTagsData?> GetArticleWithAuthorTagsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var article = await _context.Articles
        .Include(a => a.Author)
        .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
        .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    if (article == null) return null;

    var tags = article.ArticleTags.Select(at => new TagResponse(
        at.Tag.Id,
        at.Tag.Name,
        at.Tag.Slug
    )).ToList();

    return new ArticleWithAuthorTagsData(
        article.Id,
        article.AuthorId,
        article.Author.Name,
        article.Title,
        article.Slug,
        article.Content,
        article.CoverImageUrl,
        article.Status,
        article.PublishedAt,
        article.ScheduledAt,
        article.ViewCount,
        tags,
        article.CreatedAt,
        article.UpdatedAt);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetScheduledArticlesToPublishAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Author)
        .Where(a => a.Status == Enums.ArticleStatus.Scheduled && a.ScheduledAt <= DateTime.UtcNow)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> GetArticlesCountSinceAsync(DateTime since, CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .CountAsync(a => a.CreatedAt >= since, cancellationToken);
  }
}