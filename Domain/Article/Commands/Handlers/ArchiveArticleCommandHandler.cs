using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class ArchiveArticleCommandHandler(
  ArticleStoreRepository articleStoreRepository,
  ArticleQueryRepository articleQueryRepository
  ) : IRequestHandler<ArchiveArticleCommand, ArticleDto>
{

  public async Task<ArticleDto> Handle(ArchiveArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only archive your own articles");

    if (article.Status == Enums.ArticleStatus.Archived)
      throw new BadRequestException("Article is already archived");

    article.Status = Enums.ArticleStatus.Archived;
    await articleStoreRepository.SaveChangesAsync(cancellationToken);

    var articleWithAuthor = await articleQueryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article Not Found");

    return new ArticleDto(
      articleWithAuthor.Id,
      articleWithAuthor.AuthorId,
      articleWithAuthor.AuthorName,
      articleWithAuthor.Title,
      articleWithAuthor.Slug,
      articleWithAuthor.Content,
      articleWithAuthor.Summary,
      articleWithAuthor.CoverImageUrl,
      articleWithAuthor.ThumbnailId,
      null,
      articleWithAuthor.ContentImages,
      articleWithAuthor.Status,
      articleWithAuthor.PublishedAt,
      articleWithAuthor.ScheduledAt,
      articleWithAuthor.ViewCount,
      articleWithAuthor.Tags,
      articleWithAuthor.CreatedAt,
      articleWithAuthor.UpdatedAt
    );
  }

}