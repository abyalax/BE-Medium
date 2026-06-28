using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetAllPermissionsQueryHandler(PermissionQueryRepository permissionQueryRepository) : IRequestHandler<GetAllPermissionsQuery, IEnumerable<PermissionResponse>>
{
  public async Task<IEnumerable<PermissionResponse>> Handle(GetAllPermissionsQuery query, CancellationToken cancellationToken)
  {
    return await permissionQueryRepository.GetAllAsync(cancellationToken);
  }
}