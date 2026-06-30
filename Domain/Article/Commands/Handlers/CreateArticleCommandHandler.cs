using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, ArticleDto>
{
  private readonly ArticleStoreRepository _articleStoreRepository;
  private readonly ArticleQueryRepository _articleQueryRepository;
  private readonly IEventHandlerResolver _eventHandlerResolver;
  private readonly IJetStreamEventPublisher _jetStreamPublisher;

  public CreateArticleCommandHandler(
    ArticleStoreRepository articleStoreRepository,
    ArticleQueryRepository articleQueryRepository,
    IEventHandlerResolver eventHandlerResolver,
    IJetStreamEventPublisher jetStreamPublisher
  )
  {
    _articleStoreRepository = articleStoreRepository;
    _articleQueryRepository = articleQueryRepository;
    _eventHandlerResolver = eventHandlerResolver;
    _jetStreamPublisher = jetStreamPublisher;
  }

  public async Task<ArticleDto> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
  {
    var slug = GenerateSlug(command.Title);

    if (await _articleQueryRepository.ExistsBySlugAsync(slug, cancellationToken))
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

    await _articleStoreRepository.AddAsync(article, cancellationToken);
    await _articleStoreRepository.SaveChangesAsync(cancellationToken);

    if (tagIds.Count > 0)
    {
      await _articleStoreRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
      await _articleStoreRepository.SaveChangesAsync(cancellationToken);
    }

    await _eventHandlerResolver.HandleAsync(new ArticleCreatedEvent(
      article.Id.ToString(),
      command.AuthorId.ToString(),
      command.Title,
      slug,
      article.Content
    ), cancellationToken);

    // Publish to JetStream for AI summarization
    var natsEvent = new ArticleCreatedEvent(
      article.Id.ToString(),
      command.AuthorId.ToString(),
      command.Title,
      slug,
      article.Content
    );

    await _jetStreamPublisher.PublishToStreamAsync(
      NatsSubjects.ArticleCreated,
      natsEvent,
      cancellationToken
    );

    var articleWithTags = await _articleQueryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    return new ArticleDto(
      articleWithTags.Id,
      articleWithTags.AuthorId,
      articleWithTags.AuthorName,
      articleWithTags.Title,
      articleWithTags.Slug,
      articleWithTags.Content,
      articleWithTags.Summary,
      articleWithTags.CoverImageUrl,
      articleWithTags.ThumbnailId,
      null,
      articleWithTags.ContentImages,
      articleWithTags.Status,
      articleWithTags.PublishedAt,
      articleWithTags.ScheduledAt,
      articleWithTags.ViewCount,
      articleWithTags.Tags,
      articleWithTags.CreatedAt,
      articleWithTags.UpdatedAt
    );
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
}