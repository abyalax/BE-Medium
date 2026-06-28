using MediatR;

namespace Medium.Api.Domain.Auth.Commands;

public record AssignPermissionToRoleCommand(Guid RoleId, Guid PermissionId) : IRequest;