using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.User.Mapper;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Mapper;

public class CommentMapper
{
  public static CommentDto ToResponse(CommentModel comment)
  {
    // Map Article if available
    var articleDto = comment.Article == null
      ? null
      : ArticleMapper.ToResponse(comment.Article);

    // Map User if available
    var userDto = comment.User == null
      ? null
      : UserMapper.ToResponse(comment.User);

    return new CommentDto(
        comment.Id,
        comment.UserId,
        comment.ArticleId,
        comment.Content,
        userDto,
        articleDto,
        comment.CreatedAt,
        comment.UpdatedAt
    );
  }
}