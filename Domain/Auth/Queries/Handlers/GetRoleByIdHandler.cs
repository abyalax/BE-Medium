using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetRoleByIdHandler(RoleQueryRepository roleQueryRepository) : IRequestHandler<GetRoleByIdQuery, RoleResponse>
{
  public async Task<RoleResponse> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
  {
    var role = await roleQueryRepository.GetByIdWithResponseAsync(query.Id, cancellationToken);
    return role ?? throw new NotFoundException("Role not found");
  }
}