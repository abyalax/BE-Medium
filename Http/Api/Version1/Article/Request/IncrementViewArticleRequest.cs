using FluentValidation;

using Medium.Api.Domain.Article.Commands;

namespace Medium.Api.Http.Api.Version1.Article.Request;

public class IncrementArticleRequest : AbstractValidator<IncrementViewArticleCommand>
{
  public IncrementArticleRequest()
  {
    RuleFor(x => x.ArticleId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");
  }
}