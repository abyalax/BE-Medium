using MediatR;

using Medium.Api.Domain.ReadingHistory.Dtos;

namespace Medium.Api.Domain.ReadingHistory.Queries;

public record GetReadingHistoryByIdQuery(Guid ReadingHistoryId) : IRequest<ReadingHistoryDto>;