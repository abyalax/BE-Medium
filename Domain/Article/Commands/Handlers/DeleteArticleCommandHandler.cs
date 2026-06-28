using MediatR;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand, bool>
{

  private readonly ArticleStoreRepository _articleStoreRepository;
  private readonly ArticleQueryRepository _articleQueryRepository;
  public DeleteArticleCommandHandler(
    ArticleStoreRepository articleStoreRepository,
    ArticleQueryRepository articleQueryRepository
  )
  {
    _articleStoreRepository = articleStoreRepository;
    _articleQueryRepository = articleQueryRepository;
  }

  public async Task<bool> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await _articleQueryRepository.GetByIdAsync(command.ArticleId, cancellationToken)
        ?? throw new NotFoundException("Article not found");

    if (!command.IsAdmin && article.AuthorId != command.UserId)
    {
      throw new ForbiddenException("You can only delete your own articles");
    }

    _articleStoreRepository.Remove(article);
    await _articleStoreRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}