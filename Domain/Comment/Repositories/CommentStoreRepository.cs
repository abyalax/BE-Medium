using Medium.Api.Infrastructure.Database;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Repositories;

public class CommentStoreRepository(ApplicationDbContext context)
{

  public async Task AddAsync(CommentModel comment, CancellationToken cancellationToken = default)
  {
    await context.Comments.AddAsync(comment, cancellationToken);
  }

  public void Remove(CommentModel comment)
  {
    context.Comments.Remove(comment);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

}