using FluentValidation;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Article.Services;

namespace Medium.Api.Domain.Article.Module;

public static class ArticleModule
{
  public static IServiceCollection AddArticleModule(this IServiceCollection services)
  {
    services.AddScoped<ArticleRepository>();

    services.AddScoped<ArticleService>();

    services.AddScoped<IValidator<CreateArticleRequest>, CreateArticleRequestValidator>();
    services.AddScoped<IValidator<UpdateArticleRequest>, UpdateArticleRequestValidator>();
    services.AddScoped<IValidator<PublishArticleRequest>, PublishArticleRequestValidator>();

    return services;
  }
}