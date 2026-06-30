using MediatR;

namespace Medium.Api.Domain.ReadingHistory.Command;

public record DeleteReadingHistoryCommand(
  Guid UserId,
  Guid ReadingHistoryId,
  bool IsAdmin
) : IRequest<bool>;