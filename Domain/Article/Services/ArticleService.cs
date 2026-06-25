using System.Text.Json;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Article.Services;

public class ArticleService(ArticleRepository articleRepository, ILogger<ArticleService> logger, INatsPublisher publisher)
{
  private const int MaxPageSize = 100;
  private readonly ArticleRepository _articleRepository = articleRepository;

  private readonly ILogger<ArticleService> _logger = logger;
  private readonly INatsPublisher _publisher = publisher;

  private readonly string messageNotFound = "Article not found";

  public async Task<ArticleResponse> CreateAsync(
      Guid authorId,
      CreateArticleRequest request,
      CancellationToken cancellationToken = default
  )
  {

    var slug = GenerateSlug(request.Title);

    if (await _articleRepository.ExistsBySlugAsync(slug, cancellationToken))
    {
      throw new ConflictException("An article with this title already exists");
    }

    var tagIds = await ResolveTagIdsAsync(request.TagIds, cancellationToken);

    var article = new ArticleModel
    {
      Id = Guid.NewGuid(),
      AuthorId = authorId,
      Title = request.Title,
      Slug = slug,
      Content = request.Content,
      CoverImageUrl = request.CoverImageUrl,
      Status = Enums.ArticleStatus.Draft,
      ViewCount = 0
    };

    await _articleRepository.AddAsync(article, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    if (tagIds.Count > 0)
    {
      await _articleRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
      await _articleRepository.SaveChangesAsync(cancellationToken);
    }

    _logger.LogDebug("Create article: {@Article}", article);
    return await GetByIdAsync(article.Id, cancellationToken);
  }

  public async Task<ArticleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var article = await _articleRepository.GetArticleWithAuthorTagsAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(article);
  }

  public async Task<ArticleResponse> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    var article = await _articleRepository.GetBySlugAsync(slug, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(article);
  }

  public async Task<PagedArticleResponse> ListAsync(
      int page,
      int pageSize,
      Guid? authorId = null,
      string? tagSlug = null,
      string? searchTerm = null,
      Enums.ArticleStatus? status = null,
      string? sortBy = null,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = authorId.HasValue
        ? await _articleRepository.CountByAuthorAsync(authorId.Value, cancellationToken)
        : await _articleRepository.CountAsync(cancellationToken);

    var items = await _articleRepository.ListAsync(page, pageSize, authorId, tagSlug, searchTerm, status, sortBy, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedArticleResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<PagedArticleResponse> SearchAsync(
      string searchTerm,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var items = await _articleRepository.SearchPublishedAsync(searchTerm, page, pageSize, cancellationToken);
    var totalItems = items.Count; // Note: This is approximate, for accurate count we'd need a separate query
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedArticleResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<PagedArticleResponse> GetPopularAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var items = await _articleRepository.GetPopularAsync(page, pageSize, cancellationToken);
    var totalItems = await _articleRepository.CountPublishedAsync(cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedArticleResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<IReadOnlyCollection<ArticleResponse>> GetTrendingAsync(
      int limit,
      CancellationToken cancellationToken = default)
  {
    var items = await _articleRepository.GetTrendingAsync(limit, cancellationToken);
    return items.Select(ToResponse).ToList();
  }

  public async Task<PagedArticleResponse> GetPublishedAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _articleRepository.CountPublishedAsync(cancellationToken);
    var items = await _articleRepository.GetPublishedAsync(page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedArticleResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<ArticleResponse> UpdateAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      UpdateArticleRequest request,
      CancellationToken cancellationToken = default)
  {
    var article = await _articleRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && article.AuthorId != currentUserId)
    {
      throw new ForbiddenException("You can only edit your own articles");
    }

    var newSlug = GenerateSlug(request.Title);

    if (newSlug != article.Slug && await _articleRepository.ExistsBySlugAsync(newSlug, id, cancellationToken))
    {
      throw new ConflictException("An article with this title already exists");
    }

    var tagIds = await ResolveTagIdsAsync(request.TagIds, cancellationToken);

    article.Title = request.Title;
    article.Slug = newSlug;
    article.Content = request.Content;
    article.CoverImageUrl = request.CoverImageUrl;

    await _articleRepository.SaveChangesAsync(cancellationToken);

    await _articleRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(article.Id, cancellationToken);
  }

  public async Task<ArticleResponse> PublishAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      PublishArticleRequest request,
      CancellationToken cancellationToken = default
)
  {
    var article = await _articleRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && article.AuthorId != currentUserId)
    {
      throw new ForbiddenException("You can only publish your own articles");
    }

    if (article.Status == Enums.ArticleStatus.Published)
    {
      throw new BadRequestException("Article is already published");
    }

    if (article.Status == Enums.ArticleStatus.Archived)
    {
      throw new BadRequestException("Cannot publish an archived article");
    }

    if (request.ScheduledAt.HasValue)
    {
      if (request.ScheduledAt.Value <= DateTimeOffset.UtcNow)
      {
        throw new BadRequestException("Scheduled publish date must be in the future");
      }

      article.Status = Enums.ArticleStatus.Scheduled;
      article.ScheduledAt = request.ScheduledAt.Value;
      await _articleRepository.SaveChangesAsync(cancellationToken);
    }
    else
    {
      article.Status = Enums.ArticleStatus.Published;
      article.PublishedAt = DateTime.UtcNow;
      article.ScheduledAt = null;

      await _articleRepository.SaveChangesAsync(cancellationToken);

      var @event = new ArticlePublishedEvent(
          article.Id.ToString(),
          article.Title,
          article.AuthorId.ToString(),
          article.PublishedAt.Value
      );

      await _publisher.PublishAsync(NatsSubjects.ArticlePublished, @event);
      _logger.LogInformation("Published ArticlePublishedEvent for article {ArticleId}", article.Id);
    }

    return await GetByIdAsync(article.Id, cancellationToken);
  }

  public async Task<ArticleResponse> UnPublishAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      CancellationToken cancellationToken = default
)
  {

    var article = await _articleRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && article.AuthorId != currentUserId)
      throw new ForbiddenException("You can only publish your own articles");

    if (article.Status == Enums.ArticleStatus.Archived)
      throw new BadRequestException("Cannot publish an archived article");

    article.Status = Enums.ArticleStatus.Draft;
    article.PublishedAt = null;
    article.ScheduledAt = null;

    await _articleRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(article.Id, cancellationToken);
  }

  public async Task<ArticleResponse> ArchiveAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      CancellationToken cancellationToken = default
)
  {
    var article = await _articleRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && article.AuthorId != currentUserId)
    {
      throw new ForbiddenException("You can only archive your own articles");
    }

    if (article.Status == Enums.ArticleStatus.Archived)
    {
      throw new BadRequestException("Article is already archived");
    }

    article.Status = Enums.ArticleStatus.Archived;
    await _articleRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(article.Id, cancellationToken);
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      CancellationToken cancellationToken = default
)
  {
    var article = await _articleRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && article.AuthorId != currentUserId)
    {
      throw new ForbiddenException("You can only delete your own articles");
    }

    _articleRepository.Remove(article);
    await _articleRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
    if (article != null)
    {
      article.ViewCount++;
      await _articleRepository.SaveChangesAsync(cancellationToken);
    }
  }

  private async Task<IReadOnlyCollection<Guid>> ResolveTagIdsAsync(
      IReadOnlyCollection<Guid>? tagIds,
      CancellationToken cancellationToken
)
  {
    if (tagIds is null || tagIds.Count == 0)
    {
      return [];
    }

    var tags = await _articleRepository.GetTagsByIdsAsync(tagIds, cancellationToken);
    if (tags.Count != tagIds.Distinct().Count())
    {
      throw new NotFoundException("One or more tags were not found");
    }

    return tagIds.Distinct().ToArray();
  }

  private static string GenerateSlug(string title)
  {
    var slug = title.ToLowerInvariant();
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
    slug = slug.Trim('-');
    return slug;
  }

  private static ArticleResponse ToResponse(ArticleWithAuthorTagsData article)
  {
    return new ArticleResponse(
        article.Id,
        article.AuthorId,
        article.AuthorName,
        article.Title,
        article.Slug,
        article.Content,
        article.CoverImageUrl,
        article.Status,
        article.PublishedAt,
        article.ScheduledAt,
        article.ViewCount,
        article.Tags,
        article.CreatedAt,
        article.UpdatedAt);
  }

  private static ArticleResponse ToResponse(ArticleModel article)
  {
    var tags = article.ArticleTags.Select(at => new TagResponse(
        at.Tag.Id,
        at.Tag.Name,
        at.Tag.Slug
    )).ToList();

    return new ArticleResponse(
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
}