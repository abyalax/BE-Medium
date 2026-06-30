using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class UnpublishArticleHandler(
  ArticleQueryRepository queryRepository,
  ArticleStoreRepository storeRepository
  ) : IRequestHandler<UnpublishArticleCommand, ArticleDto>
{

  public async Task<ArticleDto> Handle(UnpublishArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await queryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only unpublish your own articles");

    if (article.Status == Enums.ArticleStatus.Archived)
      throw new BadRequestException("Cannot unpublish an archived article");

    article.Status = Enums.ArticleStatus.Draft;
    article.PublishedAt = null;
    article.ScheduledAt = null;

    await storeRepository.SaveChangesAsync(cancellationToken);

    var articleWithTags = await queryRepository.GetArticleWithAuthorTagsAsync(article.Id, cancellationToken)
      ?? throw new NotFoundException("Article Not Found");
    return ArticleMapper.ToResponse(articleWithTags);
  }

}