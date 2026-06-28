using MediatR;

namespace Medium.Api.Domain.Storage.Commands;

public record DeleteFileCommand(string BucketName, string ObjectName) : IRequest<bool>;