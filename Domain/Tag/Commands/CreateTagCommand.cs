using MediatR;

using Medium.Api.Domain.Tag.Dtos;

namespace Medium.Api.Domain.Tag.Commands;

public record CreateTagCommand(CreateTagRequest Request) : IRequest<TagDto>;