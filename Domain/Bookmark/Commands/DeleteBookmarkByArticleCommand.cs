using MediatR;

namespace Medium.Api.Domain.Bookmark.Commands;

public record DeleteBookmarkByArticleCommand(
  Guid UserId,
  Guid ArticleId
) : IRequest;