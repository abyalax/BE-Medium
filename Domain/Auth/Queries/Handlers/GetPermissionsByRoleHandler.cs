using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetPermissionsByRoleHandler(PermissionQueryRepository permissionQueryRepository) : IRequestHandler<GetPermissionsByRoleQuery, IEnumerable<PermissionResponse>>
{
  public async Task<IEnumerable<PermissionResponse>> Handle(GetPermissionsByRoleQuery query, CancellationToken cancellationToken)
  {
    return await permissionQueryRepository.GetByRoleIdAsync(query.RoleId, cancellationToken);
  }
}