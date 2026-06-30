using MediatR;

using Medium.Api.Domain.Follow.Dtos;

namespace Medium.Api.Domain.Follow.Queries;

public record GetFollowingQuery(Guid UserId, int Page, int PageSize) : IRequest<PagedFollowResponse>;