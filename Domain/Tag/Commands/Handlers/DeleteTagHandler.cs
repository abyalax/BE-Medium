using MediatR;

using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Tag.Commands.Handlers;

public class DeleteTagHandler(
  TagStoreRepository tagStoreRepository,
  TagQueryRepository tagQueryRepository,
  RedisService redisService
) : IRequestHandler<DeleteTagCommand>
{
  private readonly string messageNotFound = "Tag not found";

  public async Task Handle(DeleteTagCommand command, CancellationToken cancellationToken)
  {
    var tag = await tagQueryRepository.GetByIdAsync(command.Id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (await tagQueryRepository.HasArticlesAsync(command.Id, cancellationToken))
    {
      throw new BadRequestException("Cannot delete tag that is used by articles");
    }

    tagStoreRepository.Remove(tag);
    await tagStoreRepository.SaveChangesAsync(cancellationToken);

    await InvalidateTagCacheAsync(tag.Id, tag.Slug, cancellationToken);
  }

  private async Task InvalidateTagCacheAsync(Guid tagId, string slug, CancellationToken cancellationToken)
  {
    var keys = new[]
    {
      $"tag:{tagId}",
      $"tags:all"
    };
    await redisService.DeleteAsync(keys, cancellationToken);
  }
}