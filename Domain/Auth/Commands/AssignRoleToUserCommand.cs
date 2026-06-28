using MediatR;

namespace Medium.Api.Domain.Auth.Commands;

public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest;