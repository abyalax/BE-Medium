using MediatR;

using Medium.Api.Domain.Article.Dtos;

namespace Medium.Api.Domain.Article.Queries;

public record GetArticleBySlugQuery(
  string Slug
) : IRequest<ArticleDto>;