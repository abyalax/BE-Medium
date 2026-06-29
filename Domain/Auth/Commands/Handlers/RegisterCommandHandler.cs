using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Services;

using DomainUserRegisteredEvent = Medium.Api.Domain.Auth.Events.UserRegisteredEvent;
using NatsSendWelcomeEmailRequest = Medium.Api.Infrastructure.Nats.Events.SendWelcomeEmailRequest;
using NatsSendWelcomeEmailResponse = Medium.Api.Infrastructure.Nats.Events.SendWelcomeEmailResponse;
using NatsUserRegisteredEvent = Medium.Api.Infrastructure.Nats.Events.UserRegisteredEvent;
using UserModel = Medium.Api.Models.User;
using UserRoleModel = Medium.Api.Models.UserRole;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class RegisterCommandHandler(
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    UserQueryRepository userQueryRepository,
    UserStoreRepository userStoreRepository,
    IEventHandlerResolver eventHandlerResolver,
    IJetStreamEventPublisher jsPublisher,
    INatsPublisher natsPublisher,
    ILogger<RegisterCommandHandler> logger
) : IRequestHandler<RegisterCommand, AuthDto>
{
  public async Task<AuthDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
  {
    var existingUser = await userQueryRepository.FindByEmailAsync(command.Email, cancellationToken);
    if (existingUser != null)
    {
      throw new ConflictException("User with this email already exists");
    }

    var user = new UserModel
    {
      Id = Guid.NewGuid(),
      Name = command.Name,
      Email = command.Email,
      Password = passwordHasher.HashPassword(command.Password),
      Bio = command.Bio,
      AvatarUrl = command.AvatarUrl,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    await userStoreRepository.AddAsync(user, cancellationToken);

    var defaultRole = await userQueryRepository.GetRoleByNameAsync("Reader", cancellationToken);
    if (defaultRole != null)
    {
      await userStoreRepository.AddUserRoleAsync(new UserRoleModel
      {
        UserId = user.Id,
        RoleId = defaultRole.Id
      }, cancellationToken);
    }

    await userStoreRepository.SaveChangesAsync(cancellationToken);

    var userWithRoles = await userQueryRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    var token = jwtTokenGenerator.GenerateToken(
        user.Id,
        user.Email,
        userWithRoles?.Roles.Select(role => role.Name) ?? [],
        GetPermissions(userWithRoles?.Roles ?? []));

    var response = new AuthDto(
        user.Id,
        user.Name,
        user.Email,
        token,
        userWithRoles?.Roles ?? [],
        user.Bio,
        user.AvatarUrl
    );

    // Publish event to legacy internal event handler
    await eventHandlerResolver.HandleAsync(new DomainUserRegisteredEvent(user.Id, user.Email, user.Name), cancellationToken);

    // Publish user registered event to NATS JetStream
    var natsEvent = new NatsUserRegisteredEvent(user.Id, user.Email, user.Name);
    await jsPublisher.PublishToStreamAsync("user.registered", natsEvent, cancellationToken);

    // Send welcome email via request-reply pattern
    var emailRequest = new NatsSendWelcomeEmailRequest(user.Id, user.Email, user.Name);
    try
    {
      var emailResponse = await natsPublisher.RequestAsync<NatsSendWelcomeEmailRequest, NatsSendWelcomeEmailResponse>("email.send-welcome", emailRequest, cancellationToken);

      if (emailResponse == null || !emailResponse.Success)
      {
        logger.LogWarning("Email sending failed or returned null response: {Message}", emailResponse?.Message ?? "No response body");
      }
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Failed to complete request-reply welcome email sequence for user {UserId}", user.Id);
    }

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
          throw new ArgumentException("Invalid user DTO type mapping.");
      }
    }

    return permissions.Distinct(StringComparer.OrdinalIgnoreCase);
  }
}