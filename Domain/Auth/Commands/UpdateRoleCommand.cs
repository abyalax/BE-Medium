using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Commands;

public record UpdateRoleCommand(Guid Id, string Name, string Description) : IRequest<RoleResponse>;