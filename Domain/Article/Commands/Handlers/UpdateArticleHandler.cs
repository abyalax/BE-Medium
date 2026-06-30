using MediatR;

using Medium.Api.Common.Utils;
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class UpdateArticleHandler(
  ArticleQueryRepository queryRepository,
  ArticleStoreRepository storeRepository,
  IEventHandlerResolver eventHandlerResolver
  ) : IRequestHandler<UpdateArticleCommand, ArticleDto>
{

  public async Task<ArticleDto> Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await queryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only edit your own articles");

    var newSlug = Utils.GenerateSlug(command.Title ?? article.Title);

    if (newSlug != article.Slug && await queryRepository.ExistsBySlugAsync(newSlug, command.ArticleId, cancellationToken))
      throw new ConflictException("An article with this title already exists");

    var tagIds = await ResolveTagIdsAsync(command.TagIds, cancellationToken);

    if (command.Title != null)
      article.Title = command.Title;
    if (command.Content != null)
      article.Content = command.Content;
    if (command.CoverImageUrl != null)
      article.CoverImageUrl = command.CoverImageUrl;

    article.Slug = newSlug;
    article.UpdatedAt = DateTime.UtcNow;

    await storeRepository.SaveChangesAsync(cancellationToken);

    await storeRepository.ReplaceArticleTagsAsync(article.Id, tagIds, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    // Publish ArticleUpdatedEvent
    await eventHandlerResolver.HandleAsync(
      new ArticleUpdatedEvent
      {
        ArticleId = command.ArticleId.ToString(),
        AuthorId = command.UserId.ToString(),
        Title = command.Title ?? "Updated"
      },
      cancellationToken
    );

    var articleWithTags = await queryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article Not Found");
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