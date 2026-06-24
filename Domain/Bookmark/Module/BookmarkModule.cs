
using FluentValidation;

using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Domain.Bookmark.Services;

namespace Medium.Api.Domain.Bookmark.Module;

public static class BookmarkModule
{
  public static IServiceCollection AddBookmarkModule(this IServiceCollection services)
  {
    services.AddScoped<BookmarkRepository>();

    services.AddScoped<BookmarkService>();

    services.AddScoped<IValidator<BookmarkRequest>, BookmarkRequestValidator>();

    return services;
  }
}