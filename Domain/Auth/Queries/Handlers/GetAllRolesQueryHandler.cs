using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetAllRolesQueryHandler(RoleQueryRepository roleQueryRepository) : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleResponse>>
{
  public async Task<IEnumerable<RoleResponse>> Handle(GetAllRolesQuery query, CancellationToken cancellationToken)
  {
    return await roleQueryRepository.GetAllAsync(cancellationToken);
  }
}