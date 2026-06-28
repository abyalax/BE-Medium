using Medium.Api.Domain.User.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.User.Queries;

public record ListUserQuery() : PagedQuery<PaginationModel<UserDto>>;