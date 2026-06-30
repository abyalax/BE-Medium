using MediatR;

using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.ReadingHistory.Command.Handlers;

public class DeleteReadingHistoryHandler(
  ReadingHistoryStoreRepository storeRepository,
  ReadingHistoryQueryRepository queryRepository
  ) : IRequestHandler<DeleteReadingHistoryCommand, bool>
{

  public async Task<bool> Handle(DeleteReadingHistoryCommand command, CancellationToken cancellationToken)
  {
    var readingHistory = await queryRepository.GetByIdAsync(command.ReadingHistoryId, cancellationToken)
      ?? throw new NotFoundException("ReadingHistory not found");

    if (readingHistory.UserId != command.UserId && !command.IsAdmin)
      throw new ForbiddenException("You can only delete your own ReadingHistorys");

    storeRepository.Remove(readingHistory);
    await storeRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}