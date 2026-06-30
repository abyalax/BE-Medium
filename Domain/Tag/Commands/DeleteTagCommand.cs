using MediatR;

namespace Medium.Api.Domain.Tag.Commands;

public record DeleteTagCommand(Guid Id) : IRequest;