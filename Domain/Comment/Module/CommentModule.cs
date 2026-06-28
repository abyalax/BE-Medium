using Medium.Api.Domain.Comment.Repositories;

namespace Medium.Api.Domain.Comment.Module;

public static class CommentModule
{
  public static IServiceCollection AddCommentModule(this IServiceCollection services)
  {
    services.AddScoped<CommentQueryRepository>();
    services.AddScoped<CommentStoreRepository>();

    return services;
  }
}