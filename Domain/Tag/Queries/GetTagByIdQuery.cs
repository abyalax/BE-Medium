using MediatR;

using Medium.Api.Domain.Tag.Dtos;

namespace Medium.Api.Domain.Tag.Queries;

public record GetTagByIdQuery(Guid Id) : IRequest<TagDto>;