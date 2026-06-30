using Medium.Api.Domain.Article.Commands.Handlers;
using Medium.Api.Domain.Article.Queries.Handlers;
using Medium.Api.Domain.Article.Repositories;

namespace Medium.Api.Domain.Article.Module;

public static class ArticleModule
{
  public static IServiceCollection AddArticleModule(this IServiceCollection services)
  {
    services.AddScoped<ArticleQueryRepository>();
    services.AddScoped<ArticleStoreRepository>();
    return services;
  }
}