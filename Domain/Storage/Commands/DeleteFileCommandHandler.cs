using MediatR;

using Medium.Api.Infrastructure.Storage;

namespace Medium.Api.Domain.Storage.Commands;

public class DeleteFileCommandHandler(IStorageService storageService) : IRequestHandler<DeleteFileCommand, bool>
{
  public async Task<bool> Handle(DeleteFileCommand command, CancellationToken cancellationToken = default)
  {
    await storageService.DeleteFileAsync(command.BucketName, command.ObjectName, cancellationToken);
    return true;
  }
}