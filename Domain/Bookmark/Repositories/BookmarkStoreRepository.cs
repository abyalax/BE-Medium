using Medium.Api.Infrastructure.Database;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Repositories;

public class BookmarkStoreRepository(ApplicationDbContext context)
{

  public async Task AddAsync(BookmarkModel bookmark, CancellationToken cancellationToken = default)
  {
    await context.Bookmarks.AddAsync(bookmark, cancellationToken);
  }

  public void Remove(BookmarkModel bookmark)
  {
    context.Bookmarks.Remove(bookmark);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

}