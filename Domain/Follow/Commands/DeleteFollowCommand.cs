using MediatR;

namespace Medium.Api.Domain.Follow.Commands;

public record DeleteFollowCommand(Guid Id, Guid CurrentUserId) : IRequest;