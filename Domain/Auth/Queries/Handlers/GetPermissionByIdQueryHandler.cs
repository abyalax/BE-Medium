using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetPermissionByIdQueryHandler(PermissionQueryRepository permissionQueryRepository) : IRequestHandler<GetPermissionByIdQuery, PermissionResponse>
{
  public async Task<PermissionResponse> Handle(GetPermissionByIdQuery query, CancellationToken cancellationToken)
  {
    var permission = await permissionQueryRepository.GetByIdWithResponseAsync(query.Id, cancellationToken);
    return permission ?? throw new NotFoundException("Permission not found");
  }
}