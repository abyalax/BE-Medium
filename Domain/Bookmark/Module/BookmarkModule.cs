using Medium.Api.Domain.Bookmark.Commands.Handlers;
using Medium.Api.Domain.Bookmark.Queries.Handlers;
using Medium.Api.Domain.Bookmark.Repositories;

namespace Medium.Api.Domain.Bookmark.Module;

public static class BookmarkModule
{
  public static IServiceCollection AddBookmarkModule(this IServiceCollection services)
  {
    services.AddScoped<BookmarkQueryRepository>();
    services.AddScoped<BookmarkStoreRepository>();

    services.AddScoped<CreateBookmarkHandler>();
    services.AddScoped<DeleteBookmarkByArticleHandler>();
    services.AddScoped<DeleteBookmarkByIdHandler>();
    services.AddScoped<GetBookmarkByIdHandler>();
    services.AddScoped<GetBookmarkByUserHandler>();

    return services;
  }
}