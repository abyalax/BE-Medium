using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;

using DomainUserLoggedInEvent = Medium.Api.Domain.Auth.Events.UserLoggedInEvent;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class LoginCommandHandler(
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    UserQueryRepository userQueryRepository,
    IEventHandlerResolver eventHandlerResolver
  ) : IRequestHandler<LoginCommand, AuthDto>
{
  public async Task<AuthDto> Handle(LoginCommand command, CancellationToken cancellationToken)
  {
    var user = await userQueryRepository.FindByEmailWithRolePermissionAndPasswordAsync(command.Email, cancellationToken)
      ?? throw new UnauthenticatedException("Invalid email or password");
    if (!passwordHasher.VerifyPassword(command.Password, user.Password)) throw new UnauthenticatedException("Invalid email or password");

    var roles = user.Roles.Select(role => role.Name);
    var token = jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles, GetPermissions(user.Roles));

    var response = new AuthDto(
        user.Id,
        user.Name,
        user.Email,
        token,
        user.Roles,
        user.Bio,
        user.AvatarUrl
    );

    await eventHandlerResolver.HandleAsync(new DomainUserLoggedInEvent
    {
      UserId = user.Id,
      Email = user.Email
    },
      cancellationToken
    );

    return response;
  }

  // Ganti 'RoleDto' dengan nama class tipe data dari user.Roles kamu (misal: Role, RoleDto, dll.)
  private static IEnumerable<string> GetPermissions(IReadOnlyCollection<RoleWithPermissionsDto> roles)
  {
    return roles
        .SelectMany(r => r.Permissions)
        .Select(p => p.Code)
        .Distinct(StringComparer.OrdinalIgnoreCase);
  }
}