using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.User.Mapper;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Mapper;

public class ReadingHistoryMapper
{
  public static ReadingHistoryDto ToResponse(ReadingHistoryModel ReadingHistory)
  {
    // Map Article if available
    var articleDto = ReadingHistory.Article == null
      ? null
      : ArticleMapper.ToResponse(ReadingHistory.Article);

    // Map User if available
    var userDto = ReadingHistory.User == null
      ? null
      : UserMapper.ToResponse(ReadingHistory.User);

    return new ReadingHistoryDto(
      ReadingHistory.Id,
      ReadingHistory.UserId,
      ReadingHistory.ArticleId,
      ReadingHistory.DurationSeconds,
      userDto,
      articleDto,
      ReadingHistory.CreatedAt
    );
  }
}