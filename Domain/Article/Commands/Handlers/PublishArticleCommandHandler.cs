using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Events;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class PublishArticleCommandHandler : IRequestHandler<PublishArticleCommand, ArticleDto>
{
  private readonly ArticleQueryRepository _articleQueryRepository;
  private readonly ArticleStoreRepository _articleStoreRepository;
  private readonly IEventHandlerResolver _eventHandlerResolver;

  public PublishArticleCommandHandler(
    ArticleQueryRepository articleQueryRepository,
    ArticleStoreRepository articleStoreRepository,
    IEventHandlerResolver eventHandlerResolver
    )
  {
    _articleQueryRepository = articleQueryRepository;
    _articleStoreRepository = articleStoreRepository;
    _eventHandlerResolver = eventHandlerResolver;
  }

  public async Task<ArticleDto> Handle(PublishArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await _articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
        ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only publish your own articles");

    if (article.Status == Enums.ArticleStatus.Published)
      throw new BadRequestException("Article is already published");

    if (article.Status == Enums.ArticleStatus.Archived)
      throw new BadRequestException("Cannot publish an archived article");

    if (command.ScheduledAt.HasValue)
    {
      if (command.ScheduledAt.Value <= DateTimeOffset.UtcNow)
        throw new BadRequestException("Scheduled publish date must be in the future");

      article.Status = Enums.ArticleStatus.Scheduled;
      article.ScheduledAt = command.ScheduledAt.Value;
      await _articleStoreRepository.SaveChangesAsync(cancellationToken);
    }
    else
    {
      article.Status = Enums.ArticleStatus.Published;
      article.PublishedAt = DateTime.UtcNow;
      article.ScheduledAt = null;

      await _articleStoreRepository.SaveChangesAsync(cancellationToken);

      // Publish ArticlePublishedEvent
      await _eventHandlerResolver.HandleAsync(new ArticlePublishedEvent(command.ArticleId, command.UserId, article.Title, article.Slug), cancellationToken);
    }

    var articleWithTags = await _articleQueryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken);
    return ToResponse(articleWithTags!);
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