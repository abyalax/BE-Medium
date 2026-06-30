using MediatR;

using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Mapper;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.ReadingHistory.Queries.Handlers;

public class GetReadingHistoryByIdHandler(
  ReadingHistoryQueryRepository queryRepository,
  RedisService redisService
) : IRequestHandler<GetReadingHistoryByIdQuery, ReadingHistoryDto>
{

  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

  public async Task<ReadingHistoryDto> Handle(
    GetReadingHistoryByIdQuery query,
    CancellationToken cancellationToken
  )
  {
    var cacheKey = $"readingHistory:{query.ReadingHistoryId}";
    var cachedReadingHistory = await redisService.GetAsync<ReadingHistoryDto>(cacheKey, cancellationToken);

    if (cachedReadingHistory != null) return cachedReadingHistory;

    var readingHistory = await queryRepository.GetByIdAsync(query.ReadingHistoryId, cancellationToken)
      ?? throw new NotFoundException("ReadingHistory not found");

    var mappedReadingHistory = ReadingHistoryMapper.ToResponse(readingHistory);

    await redisService.SetAsync(cacheKey, mappedReadingHistory, CacheExpiry, cancellationToken);
    return mappedReadingHistory;
  }
}