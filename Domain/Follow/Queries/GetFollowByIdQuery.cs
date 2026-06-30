using MediatR;

using Medium.Api.Domain.Follow.Dtos;

namespace Medium.Api.Domain.Follow.Queries;

public record GetFollowByIdQuery(Guid Id) : IRequest<FollowResponse>;