using MediatR;

namespace Medium.Api.Domain.Follow.Commands;

public record UnfollowCommand(Guid FollowerId, Guid FollowingId) : IRequest;