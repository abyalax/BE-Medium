using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class PublishArticleCommandHandler(
  ArticleQueryRepository articleQueryRepository,
  ArticleStoreRepository articleStoreRepository,
  IJetStreamEventPublisher jetStreamPublisher
) : IRequestHandler<PublishArticleCommand, ArticleDto>
{
  private readonly ArticleQueryRepository _articleQueryRepository = articleQueryRepository;
  private readonly ArticleStoreRepository _articleStoreRepository = articleStoreRepository;

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

      var natsEvent = new ArticlePublishedEvent(
        article.Id.ToString(),
        article.AuthorId.ToString(),
        article.Title,
        article.PublishedAt
      );

      await jetStreamPublisher.PublishToStreamAsync(
        NatsSubjects.ArticlePublished,
        natsEvent,
        cancellationToken
      );

    }

    var articleWithTags = await _articleQueryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article not found");
    return articleWithTags;
  }
}