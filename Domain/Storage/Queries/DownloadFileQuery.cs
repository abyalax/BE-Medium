using MediatR;

namespace Medium.Api.Domain.Storage.Queries;

public record DownloadFileQuery(string BucketName, string ObjectName) : IRequest<Stream>;