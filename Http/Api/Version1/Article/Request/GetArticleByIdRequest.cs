namespace Medium.Api.Http.Api.Version1.Article.Request;

using FluentValidation;

using Medium.Api.Domain.Article.Queries;

public class GetArticleByIdRequest : AbstractValidator<GetArticleByIdQuery>
{
  public GetArticleByIdRequest()
  {
    RuleFor(x => x.ArticleId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be valid GUID");
  }
}