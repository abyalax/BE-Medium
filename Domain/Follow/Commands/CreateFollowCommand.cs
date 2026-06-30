using MediatR;

using Medium.Api.Domain.Follow.Dtos;

namespace Medium.Api.Domain.Follow.Commands;

public record CreateFollowCommand(Guid FollowerId, FollowRequest Request) : IRequest<FollowResponse>;