using MediatR;

using Medium.Api.Common.Utils;
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class CreateArticleHandler(
  ArticleStoreRepository storeRepository,
  ArticleQueryRepository queryRepository,
  IEventHandlerResolver eventHandlerResolver,
  IJetStreamEventPublisher jetStreamPublisher
  ) : IRequestHandler<CreateArticleCommand, ArticleDto>
{
  public async Task<ArticleDto> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
  {
    var slug = Utils.GenerateSlug(command.Title);

    if (await queryRepository.ExistsBySlugAsync(slug, cancellationToken))
      throw new ConflictException("An article with this title already exists");

    var tagIds = await ResolveTagIdsAsync(command.TagIds, cancellationToken);

    var article = new ArticleModel
    {
      Id = Guid.NewGuid(),
      AuthorId = command.AuthorId,
      Title = command.Title,
      Slug = slug,
      Content = command.Content,
      CoverImageUrl = command.CoverImageUrl,
      Status = Enums.ArticleStatus.Draft,
      ViewCount = 0
    };

    await storeRepository.AddAsync(article, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    if (tagIds.Count > 0)
    {
      await storeRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
      await storeRepository.SaveChangesAsync(cancellationToken);
    }

    await eventHandlerResolver.HandleAsync(new ArticleCreatedEvent
    {
      ArticleId = article.Id.ToString(),
      AuthorId = command.AuthorId.ToString(),
      Title = command.Title,
      Slug = slug,
      Content = article.Content
    }, cancellationToken);

    // Publish to JetStream for AI summarization
    var natsEvent = new ArticleCreatedEvent
    {
      ArticleId = article.Id.ToString(),
      AuthorId = command.AuthorId.ToString(),
      Title = command.Title,
      Slug = slug,
      Content = article.Content
    };

    await jetStreamPublisher.PublishToStreamAsync(
      NatsSubjects.ArticleCreated,
      natsEvent,
      cancellationToken
    );

    var articleWithTags = await queryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    return ArticleMapper.ToResponse(articleWithTags);

  }

  private async Task<IReadOnlyCollection<Guid>> ResolveTagIdsAsync(
      IReadOnlyCollection<Guid>? tagIds,
      CancellationToken cancellationToken)
  {
    if (tagIds is null || tagIds.Count == 0) return [];

    var tags = await queryRepository.GetTagsByIdsAsync(tagIds, cancellationToken);
    if (tags.Count != tagIds.Distinct().Count())
      throw new NotFoundException("One or more tags were not found");

    return [.. tagIds.Distinct()];
  }
}