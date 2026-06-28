using MediatR;

namespace Medium.Api.Domain.User.Commands;

public record AssignUserRoleCommand(
  Guid UserId,
  IReadOnlyCollection<Guid> RoleIds
) : IRequest;