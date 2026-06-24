namespace Medium.Api.Models;

public class Follow : Entity
{
  public Guid FollowerId { get; set; }

  public Guid FollowingId { get; set; }

  public User Follower { get; set; } = null!;

  public User Following { get; set; } = null!;
}