namespace Medium.Api.Infrastructure.Pagination;

public record PaginationMeta
{
  public int CurrentPage { get; init; }
  public int PerPage { get; init; }
  public int TotalPages { get; init; }
  public int TotalCount { get; init; }
  public bool HasPrevious => CurrentPage > 1;
  public bool HasNext => CurrentPage < TotalPages;
}