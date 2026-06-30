using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Mapper;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Commands.Handlers;

public class CreateBookmarkHandler(
  BookmarkStoreRepository storeRepository,
  BookmarkQueryRepository queryRepository
  ) : IRequestHandler<CreateBookmarkCommand, BookmarkDto>
{

  public async Task<BookmarkDto> Handle(CreateBookmarkCommand command, CancellationToken cancellationToken)
  {

    if (await queryRepository.ExistsAsync(command.UserId, command.ArticleId, cancellationToken))
      throw new ConflictException("Article is already bookmarked");

    var bookmark = new BookmarkModel
    {
      Id = Guid.NewGuid(),
      UserId = command.UserId,
      ArticleId = command.ArticleId
    };

    await storeRepository.AddAsync(bookmark, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    var bookmarkResponse = await queryRepository.GetByIdAsync(bookmark.Id, cancellationToken)
      ?? throw new NotFoundException("Bookmark not found");
    return BookmarkMapper.ToResponse(bookmarkResponse);

  }
}