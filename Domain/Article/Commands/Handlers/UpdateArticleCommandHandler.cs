using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Events;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, ArticleDto>
{
  private readonly ArticleQueryRepository _articleQueryRepository;
  private readonly ArticleStoreRepository _articleStoreRepository;
  private readonly IEventHandlerResolver _eventHandlerResolver;

  public UpdateArticleCommandHandler(
    ArticleQueryRepository articleQueryRepository,
    ArticleStoreRepository articleStoreRepository,
    IEventHandlerResolver eventHandlerResolver
  )
  {
    _articleQueryRepository = articleQueryRepository;
    _articleStoreRepository = articleStoreRepository;
    _eventHandlerResolver = eventHandlerResolver;
  }

  public async Task<ArticleDto> Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await _articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
        ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
    {
      throw new ForbiddenException("You can only edit your own articles");
    }

    var newSlug = GenerateSlug(command.Title ?? article.Title);

    if (newSlug != article.Slug && await _articleQueryRepository.ExistsBySlugAsync(newSlug, command.ArticleId, cancellationToken))
    {
      throw new ConflictException("An article with this title already exists");
    }

    var tagIds = await ResolveTagIdsAsync(command.TagIds, cancellationToken);

    if (command.Title != null)
      article.Title = command.Title;
    if (command.Content != null)
      article.Content = command.Content;
    if (command.CoverImageUrl != null)
      article.CoverImageUrl = command.CoverImageUrl;

    article.Slug = newSlug;
    article.UpdatedAt = DateTime.UtcNow;

    await _articleStoreRepository.SaveChangesAsync(cancellationToken);

    await _articleStoreRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
    await _articleStoreRepository.SaveChangesAsync(cancellationToken);

    // Publish ArticleUpdatedEvent
    await _eventHandlerResolver.HandleAsync(new ArticleUpdatedEvent(command.ArticleId, command.UserId, command.Title ?? "Updated"), cancellationToken);

    var articleWithTags = await _articleQueryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken);
    return ToResponse(articleWithTags!);
  }

  private async Task<IReadOnlyCollection<Guid>> ResolveTagIdsAsync(
      IReadOnlyCollection<Guid>? tagIds,
      CancellationToken cancellationToken)
  {
    if (tagIds is null || tagIds.Count == 0)
    {
      return [];
    }

    var tags = await _articleQueryRepository.GetTagsByIdsAsync(tagIds, cancellationToken);
    if (tags.Count != tagIds.Distinct().Count())
    {
      throw new NotFoundException("One or more tags were not found");
    }

    return [.. tagIds.Distinct()];
  }

  private static string GenerateSlug(string title)
  {
    var slug = title.ToLowerInvariant();
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
    slug = slug.Trim('-');
    return slug;
  }

  private static ArticleDto ToResponse(ArticleDto article)
  {
    return new ArticleDto(
        article.Id,
        article.AuthorId,
        article.AuthorName,
        article.Title,
        article.Slug,
        article.Content,
        article.CoverImageUrl,
        article.ThumbnailId,
        null,
        article.ContentImages,
        article.Status,
        article.PublishedAt,
        article.ScheduledAt,
        article.ViewCount,
        article.Tags,
        article.CreatedAt,
        article.UpdatedAt);
  }
}