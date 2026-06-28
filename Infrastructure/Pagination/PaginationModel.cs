namespace Medium.Api.Infrastructure.Pagination;

public class PaginationModel<T>
{
  public List<T> Data { get; set; } = new();
  public PaginationMeta Meta { get; set; } = new();
}