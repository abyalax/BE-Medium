using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Infrastructure.Pagination;

public static class PaginationExtensions
{
  public static async Task<PaginationModel<T>> ToPaginationAsync<T>(
      this IQueryable<T> query,
      int page,
      int perPage,
      CancellationToken cancellationToken = default)
  {
    var totalCount = await query.CountAsync(cancellationToken);
    var totalPages = (int)Math.Ceiling(totalCount / (double)perPage);

    var data = await query
        .Skip((page - 1) * perPage)
        .Take(perPage)
        .ToListAsync(cancellationToken);

    return new PaginationModel<T>
    {
      Data = data,
      Meta = new PaginationMeta
      {
        CurrentPage = page,
        PerPage = perPage,
        TotalPages = totalPages,
        TotalCount = totalCount
      }
    };
  }

  public static IQueryable<T> ApplySorting<T>(
      this IQueryable<T> query,
      string? sortBy,
      string order = "asc")
  {
    if (string.IsNullOrWhiteSpace(sortBy))
      return query;

    var isDescending = order?.ToLower() == "desc";

    // Use dynamic sorting via System.Linq.Dynamic.Core if needed
    // For now, this is a placeholder for custom sorting logic
    return query;
  }
}