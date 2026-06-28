using MediatR;

namespace Medium.Api.Infrastructure.Pagination;

public abstract record PagedQuery
{
  private int _page = 1;
  private int _perPage = 10;
  private string? _sortBy;
  private string _order = "asc";

  public string Search { get; set; } = string.Empty;

  public int Page
  {
    get => _page;
    set => _page = value < 1 ? 1 : value;
  }

  public int PerPage
  {
    get => _perPage;
    set => _perPage = value <= 0 ? 10 : value;
  }

  public string? SortBy
  {
    get => _sortBy;
    set => _sortBy = string.IsNullOrWhiteSpace(value) ? null : value;
  }

  public string Order
  {
    get => _order;
    set => _order = value?.ToLower() == "desc" ? "desc" : "asc";
  }
}

public abstract record PagedQuery<TResponse> : PagedQuery, IRequest<TResponse> { }