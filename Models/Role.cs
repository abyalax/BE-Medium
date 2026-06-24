namespace Medium.Api.Models;

public class Role : Entity
{

  public string Name { get; set; } = null!;

  public string Description { get; set; } = null!;
  public ICollection<UserRole> UserRoles { get; set; } = [];

  public ICollection<RolePermission> RolePermissions { get; set; } = [];
}