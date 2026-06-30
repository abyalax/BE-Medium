using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class UnpublishArticleCommandHandler : IRequestHandler<UnpublishArticleCommand, ArticleDto>
{
  private readonly ArticleQueryRepository _articleQueryRepository;
  private readonly ArticleStoreRepository _articleStoreRepository;

  public UnpublishArticleCommandHandler(
    ArticleQueryRepository articleQueryRepository,
    ArticleStoreRepository articleStoreRepository
  )
  {
    _articleQueryRepository = articleQueryRepository;
    _articleStoreRepository = articleStoreRepository;
  }

  public async Task<ArticleDto> Handle(UnpublishArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await _articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
        ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only unpublish your own articles");

    if (article.Status == Enums.ArticleStatus.Archived)
      throw new BadRequestException("Cannot unpublish an archived article");

    article.Status = Enums.ArticleStatus.Draft;
    article.PublishedAt = null;
    article.ScheduledAt = null;

    await _articleStoreRepository.SaveChangesAsync(cancellationToken);

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
        article.Summary,
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