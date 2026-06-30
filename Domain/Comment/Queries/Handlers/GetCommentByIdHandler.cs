using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Mapper;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Comment.Queries.Handlers;

public class GetCommentByIdHandler(
  CommentQueryRepository queryRepository,
  RedisService redisService
) : IRequestHandler<GetCommentByIdQuery, CommentDto>
{

  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

  public async Task<CommentDto> Handle(
      GetCommentByIdQuery query,
      CancellationToken cancellationToken
  )
  {
    var cacheKey = $"comment:{query.CommentId}";
    var cachedComment = await redisService.GetAsync<CommentDto>(cacheKey, cancellationToken);

    if (cachedComment != null) return cachedComment;

    var comment = await queryRepository.GetByIdAsync(query.CommentId, cancellationToken)
        ?? throw new NotFoundException("Comment not found");

    var mappedComment = CommentMapper.ToResponse(comment);

    await redisService.SetAsync(cacheKey, mappedComment, CacheExpiry, cancellationToken);
    return mappedComment;
  }
}