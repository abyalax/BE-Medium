using MediatR;

using Medium.Api.Domain.Article.Repositories;

namespace Medium.Api.Domain.Article.Commands.Handlers;

public class IncrementViewArticleHandler(
  ArticleStoreRepository storeRepository,
  ArticleQueryRepository queryRepository
  ) : IRequestHandler<IncrementViewArticleCommand>
{
  public async Task Handle(IncrementViewArticleCommand command, CancellationToken cancellationToken)
  {
    var article = await queryRepository.GetByIdAsync(command.ArticleId, cancellationToken);
    if (article != null)
    {
      article.ViewCount++;
      await storeRepository.SaveChangesAsync(cancellationToken);
    }
  }

}