using MediatR;

using Medium.Api.Domain.Storage.Events;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Storage;

namespace Medium.Api.Domain.Storage.Commands.Handlers;

public class UploadFileHandler(
    IStorageService storageService,
    IEventHandlerResolver eventHandlerResolver,
    ILogger<UploadFileHandler> logger) : IRequestHandler<UploadFileCommand, UploadedFileResponse>
{
  public async Task<UploadedFileResponse> Handle(UploadFileCommand command, CancellationToken cancellationToken)
  {
    var objectName = $"{Guid.NewGuid()}_{command.FileName}";

    await storageService.UploadFileAsync(
        command.FileStream,
        command.BucketName,
        objectName,
        command.ContentType,
        command.Metadata,
        cancellationToken);

    var presignedUrl = await storageService.GeneratePresignedUrlAsync(
        command.BucketName,
        objectName,
        expiresIn: 3600,
        cancellationToken);

    var response = new UploadedFileResponse(
        objectName,
        presignedUrl,
        command.FileStream.Length,
        DateTime.UtcNow
    );

    // Publish event
    await eventHandlerResolver.HandleAsync(new FileUploadedEvent(objectName, command.BucketName, command.ContentType), cancellationToken);

    logger.LogInformation("File uploaded: {ObjectName} to bucket {BucketName}", objectName, command.BucketName);

    return response;
  }
}