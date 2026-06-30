using MediatR;

using Medium.Api.Domain.Tag.Dtos;

namespace Medium.Api.Domain.Tag.Commands;

public record UpdateTagCommand(Guid Id, UpdateTagRequest Request) : IRequest<TagDto>;