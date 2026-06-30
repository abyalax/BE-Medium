using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.ReadingHistory.Queries;

public record ListReadingHistoryByUserQuery(
  Guid UserId
) : PagedQuery<PaginationModel<ReadingHistoryDto>>;