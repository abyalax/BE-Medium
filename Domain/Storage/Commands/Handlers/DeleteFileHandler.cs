using MediatR;

using Medium.Api.Infrastructure.Storage;

namespace Medium.Api.Domain.Storage.Commands.Handlers;

public class DeleteFileHandler(IStorageService storageService) : IRequestHandler<DeleteFileCommand, bool>
{
  public async Task<bool> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
  {
    await storageService.DeleteFileAsync(command.BucketName, command.ObjectName, cancellationToken);
    return true;
  }
}