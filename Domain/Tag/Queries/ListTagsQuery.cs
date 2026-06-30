using MediatR;

using Medium.Api.Domain.Tag.Dtos;

namespace Medium.Api.Domain.Tag.Queries;

public record ListTagsQuery(int Page, int PageSize) : IRequest<PagedTagDto>;