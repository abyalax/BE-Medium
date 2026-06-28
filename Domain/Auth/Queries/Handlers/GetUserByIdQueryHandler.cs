using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Auth.Queries.Handlers;

public class GetUserByIdQueryHandler(
  UserQueryRepository queryRepository
) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
  public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
  {
    var user = await queryRepository.GetByIdWWithRolePermissionAsync(query.UserId, cancellationToken);
    return user ?? throw new NotFoundException("User not found");
  }
}