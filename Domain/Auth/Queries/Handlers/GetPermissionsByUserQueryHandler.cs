using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetPermissionsByUserQueryHandler(PermissionQueryRepository permissionQueryRepository) : IRequestHandler<GetPermissionsByUserQuery, IEnumerable<PermissionResponse>>
{
  public async Task<IEnumerable<PermissionResponse>> Handle(GetPermissionsByUserQuery query, CancellationToken cancellationToken)
  {
    return await permissionQueryRepository.GetByUserIdAsync(query.UserId, cancellationToken);
  }
}