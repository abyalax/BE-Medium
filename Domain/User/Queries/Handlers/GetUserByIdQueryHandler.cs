using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.User.Queries.Handlers;

public class GetUserByIdQueryHandler(UserQueryRepository userQueryRepository) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
  public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
  {
    var user = await userQueryRepository.GetByIdAsync(query.UserId, cancellationToken)
        ?? throw new NotFoundException("User not found");

    var userWithRoles = await userQueryRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    return userWithRoles;
  }
}