using MediatR;

namespace Medium.Api.Domain.User.Commands;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;