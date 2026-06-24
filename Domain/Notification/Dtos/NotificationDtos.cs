namespace Medium.Api.Domain.Notification.Dtos;

public enum NotificationType
{
  ArticlePublished,
  CommentCreated,
  UserFollowed,
  Mention
}

public record CreateNotificationRequest(
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? RelatedEntityId = null,
    string? ActionUrl = null
);

public record UpdateNotificationRequest(
    bool IsRead
);

public record NotificationResponse(
    string Id,
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? RelatedEntityId,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt
);

public record PagedResponse<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize
);