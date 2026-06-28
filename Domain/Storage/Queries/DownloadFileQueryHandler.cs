using MediatR;

using Medium.Api.Infrastructure.Storage;

namespace Medium.Api.Domain.Storage.Queries;

public class DownloadFileQueryHandler(IStorageService storageService) : IRequestHandler<DownloadFileQuery, Stream>
{
  public async Task<Stream> Handle(DownloadFileQuery query, CancellationToken cancellationToken = default)
  {
    return await storageService.DownloadFileAsync(query.BucketName, query.ObjectName, cancellationToken);
  }
}