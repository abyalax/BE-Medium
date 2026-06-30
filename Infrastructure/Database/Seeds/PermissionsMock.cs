using System.Security.Cryptography;
using System.Text;

using PermissionConstant = Medium.Api.Common.Constant.Permissions;

namespace Medium.Api.Infrastructure.Database.Seeds;

public sealed record PermissionMock(string Code, string Name, string Description)
{
  // Generates a deterministic Guid based on the permission unique Code
  public Guid Id => CreateDeterministicGuid(Code);

  /// <summary>
  /// Generates a deterministic GUID based on an input string.
  /// </summary>
  private static Guid CreateDeterministicGuid(string input)
  {
    // Using a fixed namespace GUID for Medium permissions
    Guid namespaceId = new("b9bc7ba8-69cb-4eb6-9b87-d4fa991739c3");

    byte[] namespaceBytes = namespaceId.ToByteArray();
    byte[] inputBytes = Encoding.UTF8.GetBytes(input);

    // Swap byte order to match RFC 4122 UUID specifications
    SwapByteOrder(namespaceBytes);

    byte[] hash;
    using (IncrementalHash algorithm = IncrementalHash.CreateHash(HashAlgorithmName.SHA1))
    {
      algorithm.AppendData(namespaceBytes);
      algorithm.AppendData(inputBytes);
      hash = algorithm.GetHashAndReset();
    }

    byte[] newGuidBytes = new byte[16];
    Array.Copy(hash, 0, newGuidBytes, 0, 16);

    // Set version to 5 (SHA-1 based deterministic UUID)
    newGuidBytes[6] = (byte)((newGuidBytes[6] & 0x0F) | 0x50);
    // Set variant to RFC 4122
    newGuidBytes[8] = (byte)((newGuidBytes[8] & 0x3F) | 0x80);

    SwapByteOrder(newGuidBytes);
    return new Guid(newGuidBytes);
  }

  private static void SwapByteOrder(byte[] guidBytes)
  {
    Swap(guidBytes, 0, 3);
    Swap(guidBytes, 1, 2);
    Swap(guidBytes, 4, 5);
    Swap(guidBytes, 6, 7);
  }

  private static void Swap(byte[] array, int left, int right)
  {
    (array[right], array[left]) = (array[left], array[right]);
  }

  public static readonly IReadOnlyCollection<PermissionMock> ListPermissionsSeed =
  [
    new(PermissionConstant.Articles.Get, "Read articles", "Read published articles"),

    new(PermissionConstant.Bookmarks.Create, "Create bookmark articles", "Create bookmark published articles"),
    new(PermissionConstant.Bookmarks.Read, "Read bookmark articles", "Read bookmark published articles"),
    new(PermissionConstant.Bookmarks.ReadOwn, "Read Own bookmark articles", "Read Own bookmark published articles"),
    new(PermissionConstant.Bookmarks.Update, "Update bookmark articles", "Update Bookmark published articles"),
    new(PermissionConstant.Bookmarks.UpdateOwn, "Update own bookmark articles", "Update Bookmark Own published articles"),
    new(PermissionConstant.Bookmarks.Delete, "Delete bookmark articles", "Delete bookmark articles"),
    new(PermissionConstant.Bookmarks.DeleteOwn, "Delete own bookmark articles", "Delete own bookmark articles"),

    new(PermissionConstant.Authors.Follow, "Follow authors", "Follow article authors"),
    new(PermissionConstant.Authors.UnFollow, "UnFollow authors", "UnFollow article authors"),
    new(PermissionConstant.Authors.GetProfile, "Read author profiles", "View author profile pages"),

    new(PermissionConstant.ReadingHistory.Read, "Read Reading history", "View reading history"),
    new(PermissionConstant.ReadingHistory.ReadOwn, "Read Own Reading history", "View own reading history"),
    new(PermissionConstant.ReadingHistory.Create, "Create Reading history", "Create own reading history"),
    new(PermissionConstant.ReadingHistory.Delete, "Delete Reading history", "Delete reading history"),
    new(PermissionConstant.ReadingHistory.DeleteOwn, "Delete Own Reading history", "Delete own reading history"),

    new(PermissionConstant.Comments.Create, "Create comments", "Comment on articles"),
    new(PermissionConstant.Comments.Read, "Read comments", "Read Comment on articles"),
    new(PermissionConstant.Comments.Update, "Update comments", "Update Comment on articles"),
    new(PermissionConstant.Comments.Delete, "Delete comments", "Delete Comment on articles"),

    new(PermissionConstant.Articles.Create, "Create articles", "Create articles"),
    new(PermissionConstant.Articles.UpdateOwn, "Edit own articles", "Edit owned articles"),
    new(PermissionConstant.Articles.Publish, "Publish articles", "Publish owned articles"),
    new(PermissionConstant.Articles.Archive, "Archive articles", "Archive owned articles"),
    new(PermissionConstant.Articles.DeleteOwn, "Delete own articles", "Delete owned articles"),
    new(PermissionConstant.Analytics.GetOwn, "Own article analytics", "View analytics for owned articles"),

    new(PermissionConstant.Users.Get, "Read users", "View users"),
    new(PermissionConstant.Users.Create, "Create users", "Create users"),
    new(PermissionConstant.Users.Update, "Update users", "Update users"),
    new(PermissionConstant.Users.Delete, "Delete users", "Delete users"),
    new(PermissionConstant.Users.AssignRoles, "Assign user roles", "Assign roles to users"),

    new(PermissionConstant.Roles.Get, "Read roles", "View roles"),
    new(PermissionConstant.Roles.Create, "Create roles", "Create roles"),
    new(PermissionConstant.Roles.Update, "Update roles", "Update roles"),
    new(PermissionConstant.Roles.Delete, "Delete roles", "Delete roles"),
    new(PermissionConstant.Roles.AssignPermissions, "Assign role permissions", "Assign permissions to roles"),
    new(PermissionConstant.Roles.AssignUsers, "Assign user roles", "Assign roles to users"),

    new(PermissionConstant.PermissionsModule.Get, "Read permissions", "View permissions"),
    new(PermissionConstant.PermissionsModule.Create, "Create permissions", "Create permissions"),
    new(PermissionConstant.PermissionsModule.Update, "Update permissions", "Update permissions"),
    new(PermissionConstant.PermissionsModule.Delete, "Delete permissions", "Delete permissions"),

    new(PermissionConstant.Tags.Create, "Create tags", "Create article tags"),
    new(PermissionConstant.Tags.Read, "Read tags", "Read article tags"),
    new(PermissionConstant.Tags.Update, "Update tags", "Update article tags"),
    new(PermissionConstant.Tags.Delete, "Delete tags", "Delete article tags"),

    new(PermissionConstant.Articles.Moderate, "Moderate articles", "Moderate article content"),
    new(PermissionConstant.Articles.DeleteAny, "Delete any article", "Delete any article"),
    new(PermissionConstant.Analytics.GetSystem, "System analytics", "Access system analytics"),
    new(PermissionConstant.Admin.ManageData, "Manage administrative data", "Manage administrative data")
  ];
}