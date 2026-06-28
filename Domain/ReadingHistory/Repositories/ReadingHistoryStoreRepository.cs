using Medium.Api.Infrastructure.Database;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Repositories;

public class ReadingHistoryStoreRepository(ApplicationDbContext context)
{
  public async Task AddAsync(ReadingHistoryModel readingHistory, CancellationToken cancellationToken = default)
  {
    await context.ReadingHistories.AddAsync(readingHistory, cancellationToken);
  }

  public void Remove(ReadingHistoryModel readingHistory)
  {
    context.ReadingHistories.Remove(readingHistory);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

}