using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Notification.Queries;

public record ListUserNotificationQuery(
  Guid UserId
) : PagedQuery<PaginationModel<NotificationDto>>;