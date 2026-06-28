
using Medium.Api.Domain.Bookmark.Repositories;

namespace Medium.Api.Domain.Bookmark.Module;

public static class BookmarkModule
{
  public static IServiceCollection AddBookmarkModule(this IServiceCollection services)
  {
    services.AddScoped<BookmarkQueryRepository>();
    services.AddScoped<BookmarkStoreRepository>();

    return services;
  }
}