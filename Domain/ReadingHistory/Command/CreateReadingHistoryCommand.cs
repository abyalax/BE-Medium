using MediatR;

using Medium.Api.Domain.ReadingHistory.Dtos;

namespace Medium.Api.Domain.ReadingHistory.Command;

public record CreateReadingHistoryCommand(
  Guid UserId,
  Guid ArticleId,
  int DurationSeconds
) : IRequest<ReadingHistoryDto>;