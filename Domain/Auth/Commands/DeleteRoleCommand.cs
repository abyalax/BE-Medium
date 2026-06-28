using MediatR;

namespace Medium.Api.Domain.Auth.Commands;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;