namespace Medium.Api.Models;

public class Permission : Entity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}