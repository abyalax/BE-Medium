using MediatR;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class DeleteArticleHandler(
  ArticleStoreRepository articleStoreRepository,
  ArticleQueryRepository articleQueryRepository
  ) : IRequestHandler<DeleteArticleCommand, bool>
{

  public async Task<bool> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
      ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
      throw new ForbiddenException("You can only delete your own articles");

    articleStoreRepository.Remove(article);
    await articleStoreRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}