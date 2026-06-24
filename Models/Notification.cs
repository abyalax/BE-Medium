using Medium.Api.Enums;

namespace Medium.Api.Models;

public class Notification : Entity
{
    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}