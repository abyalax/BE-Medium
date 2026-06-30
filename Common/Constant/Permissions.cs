namespace Medium.Api.Common.Constant;

public static class Permissions
{
  public static readonly IReadOnlyCollection<string> All =
  [
    Users.Get, Users.Create, Users.Update, Users.Delete, Users.AssignRoles, Users.GetProfile, Users.UpdateProfile, Users.DeleteAccount,

    Roles.Get, Roles.Create, Roles.Update, Roles.Delete, Roles.AssignPermissions, Roles.AssignUsers,

    PermissionsModule.Get, PermissionsModule.Create, PermissionsModule.Update, PermissionsModule.Delete,

    Articles.Get, Articles.Create, Articles.UpdateOwn, Articles.DeleteOwn, Articles.Publish, Articles.Archive, Articles.Moderate, Articles.DeleteAny,

    Bookmarks.Create, Bookmarks.CreateOwn, Bookmarks.Delete, Bookmarks.DeleteOwn, Bookmarks.Update, Bookmarks.UpdateOwn,

    Authors.Follow, Authors.UnFollow, Authors.GetProfile, Comments.Create,

    ReadingHistory.Create, ReadingHistory.Read, ReadingHistory.Delete,

    Tags.Create, Analytics.GetOwn, Analytics.GetSystem, Admin.ManageData
  ];

  public static class Users
  {
    public const string Get = "users.get";
    public const string GetProfile = "users.get_profile";
    public const string Create = "users.create";
    public const string Update = "users.update";
    public const string UpdateProfile = "users.update_profile";
    public const string Delete = "users.delete";
    public const string DeleteAccount = "users.delete_account";
    public const string AssignRoles = "users.assign_roles";
  }

  public static class Roles
  {
    public const string Get = "roles.get";
    public const string Create = "roles.create";
    public const string Update = "roles.update";
    public const string Delete = "roles.delete";
    public const string AssignPermissions = "roles.assign_permissions";
    public const string AssignUsers = "roles.assign_users";
  }

  public static class PermissionsModule
  {
    public const string Get = "permissions.get";
    public const string Create = "permissions.create";
    public const string Update = "permissions.update";
    public const string Delete = "permissions.delete";
  }

  public static class Articles
  {
    public const string Get = "articles.get";
    public const string Create = "articles.create";
    public const string UpdateOwn = "articles.update_own";
    public const string DeleteOwn = "articles.delete_own";
    public const string Publish = "articles.publish";
    public const string Archive = "articles.archive";
    public const string Moderate = "articles.moderate";
    public const string DeleteAny = "articles.delete_any";
  }

  public static class Bookmarks
  {
    public const string Create = "bookmarks.create";
    public const string Read = "bookmarks.read";
    public const string Update = "bookmarks.update";
    public const string Delete = "bookmarks.delete";

    public const string CreateOwn = "bookmarks.create_own";
    public const string UpdateOwn = "bookmarks.update_own";
    public const string ReadOwn = "bookmarks.read_own";
    public const string DeleteOwn = "bookmarks.delete_own";
  }

  public static class Authors
  {
    public const string Follow = "authors.follow";
    public const string UnFollow = "authors.unfollow";
    public const string GetProfile = "authors.get_profile";
  }

  public static class Comments
  {
    public const string Create = "comments.create";
    public const string Read = "comments.read";
    public const string Update = "comments.update";
    public const string Delete = "comments.delete";
  }

  public static class ReadingHistory
  {
    public const string Create = "reading_history.create";
    public const string Read = "reading_history.read";
    public const string ReadOwn = "reading_history.read_own";
    public const string Delete = "reading_history.delete";
    public const string DeleteOwn = "reading_history.delete_own";
  }

  public static class Tags
  {
    public const string Create = "tags.Create";
    public const string Read = "tags.read";
    public const string Update = "tags.update";
    public const string Delete = "tags.delete";
  }

  public static class Analytics
  {
    public const string GetOwn = "analytics.get_own";
    public const string GetSystem = "analytics.get_system";
  }

  public static class Admin
  {
    public const string ManageData = "admin.manage_data";
  }
}