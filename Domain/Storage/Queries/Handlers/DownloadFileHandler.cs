using MediatR;

using Medium.Api.Infrastructure.Storage;

namespace Medium.Api.Domain.Storage.Queries.Handlers;

public class DownloadFileHandler(IStorageService storageService) : IRequestHandler<DownloadFileQuery, Stream>
{
  public async Task<Stream> Handle(DownloadFileQuery query, CancellationToken cancellationToken)
  {
    return await storageService.DownloadFileAsync(query.BucketName, query.ObjectName, cancellationToken);
  }
}