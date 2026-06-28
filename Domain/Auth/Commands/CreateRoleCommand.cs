using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Commands;

public record CreateRoleCommand(string Name, string Description) : IRequest<RoleResponse>;