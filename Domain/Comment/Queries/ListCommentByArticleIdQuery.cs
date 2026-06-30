using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Comment.Queries;

public record ListCommentByArticleIdQuery(
  Guid ArticleId
) : PagedQuery<PaginationModel<CommentDto>>;