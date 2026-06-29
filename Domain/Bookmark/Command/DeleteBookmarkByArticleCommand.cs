using MediatR;

namespace Medium.Api.Domain.Bookmark.Command;

public record DeleteBookmarkByArticleCommand(
  Guid UserId,
  Guid ArticleId
) : IRequest;