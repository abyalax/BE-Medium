using MediatR;

using Medium.Api.Domain.Comment.Dtos;

namespace Medium.Api.Domain.Comment.Queries;

public record GetCommentByIdQuery(Guid CommentId) : IRequest<CommentDto>;