using MediatR;

using Medium.Api.Domain.Follow.Dtos;

namespace Medium.Api.Domain.Follow.Queries;

public record GetFollowersQuery(Guid UserId, int Page, int PageSize) : IRequest<PagedFollowResponse>;