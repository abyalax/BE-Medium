namespace Medium.Api.Http.Api.Version1.Article.Request;

using FluentValidation;

using Medium.Api.Domain.Article.Queries;

public class GetArticleBySlugRequest : AbstractValidator<GetArticleBySlugQuery>
{
  public GetArticleBySlugRequest()
  {
    RuleFor(x => x.Slug).NotEmpty().WithMessage("Article Slug is required");
  }
}