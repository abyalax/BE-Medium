
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