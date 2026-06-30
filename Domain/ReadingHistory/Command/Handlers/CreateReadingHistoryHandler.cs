using MediatR;

using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Mapper;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Command.Handlers;

public class CreateReadingHistoryHandler(
  ReadingHistoryStoreRepository storeRepository,
  ReadingHistoryQueryRepository queryRepository
  ) : IRequestHandler<CreateReadingHistoryCommand, ReadingHistoryDto>
{

  public async Task<ReadingHistoryDto> Handle(CreateReadingHistoryCommand command, CancellationToken cancellationToken)
  {
    var readingHistory = new ReadingHistoryModel
    {
      Id = Guid.NewGuid(),
      UserId = command.UserId,
      ArticleId = command.ArticleId,
      DurationSeconds = command.DurationSeconds,
      ReadAt = DateTime.UtcNow
    };

    await storeRepository.AddAsync(readingHistory, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    var createdReadingHistory = await queryRepository.GetByIdAsync(readingHistory.Id, cancellationToken)
      ?? throw new NotFoundException("Reading History not found");
    return ReadingHistoryMapper.ToResponse(createdReadingHistory);
  }
}