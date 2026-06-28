using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Services;

using DomainUserLoggedInEvent = Medium.Api.Domain.Auth.Events.UserLoggedInEvent;
using NatsUserLoggedInEvent = Medium.Api.Infrastructure.Nats.Events.UserLoggedInEvent;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class LoginCommandHandler(
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    UserQueryRepository userQueryRepository,
    IEventHandlerResolver eventHandlerResolver,
    IJetStreamEventPublisher jsPublisher
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

    await eventHandlerResolver.HandleAsync(new DomainUserLoggedInEvent(user.Id, user.Email), cancellationToken);

    var natsEvent = new NatsUserLoggedInEvent(user.Id, user.Email, DateTime.UtcNow);
    await jsPublisher.PublishToStreamAsync("USER_EVENTS", "user.logged-in", natsEvent);

    return response;
  }

  private static IEnumerable<string> GetPermissions(IReadOnlyCollection<object> users)
  {
    var permissions = new List<string>();

    foreach (var user in users)
    {
      switch (user)
      {
        case UserDto u:
          permissions.AddRange(u.Roles.SelectMany(r => r.Permissions).Select(p => p.Code));
          break;

        case UserWithPasswordDto u:
          permissions.AddRange(u.Roles.SelectMany(r => r.Permissions).Select(p => p.Code));
          break;

        default:
          throw new ArgumentException("Invalid user DTO type");
      }
    }

    return permissions.Distinct(StringComparer.OrdinalIgnoreCase);
  }
}