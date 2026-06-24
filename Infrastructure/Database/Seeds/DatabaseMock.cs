using Bogus;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Models;
using AuthPermissions = Medium.Api.Infrastructure.Auth.Permissions;

namespace Medium.Api.Infrastructure.Database.Seeds;

public static class DatabaseMockData
{
    public const string DefaultPassword = "Password123!";
    public static readonly Guid ReaderRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AuthorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly IReadOnlyCollection<PermissionSeed> Permissions =
    [
        new("aaaaaaaa-0000-0000-0000-000000000001", AuthPermissions.Articles.Get, "Read articles", "Read published articles"),
        new("aaaaaaaa-0000-0000-0000-000000000002", AuthPermissions.Bookmarks.Create, "Bookmark articles", "Bookmark published articles"),
        new("aaaaaaaa-0000-0000-0000-000000000003", AuthPermissions.Authors.Follow, "Follow authors", "Follow article authors"),
        new("aaaaaaaa-0000-0000-0000-000000000004", AuthPermissions.Comments.Create, "Create comments", "Comment on articles"),
        new("aaaaaaaa-0000-0000-0000-000000000005", AuthPermissions.Authors.GetProfile, "Read author profiles", "View author profile pages"),
        new("aaaaaaaa-0000-0000-0000-000000000006", AuthPermissions.ReadingHistory.Get, "Read history", "View own reading history"),
        new("aaaaaaaa-0000-0000-0000-000000000007", AuthPermissions.Articles.Create, "Create articles", "Create articles"),
        new("aaaaaaaa-0000-0000-0000-000000000008", AuthPermissions.Articles.UpdateOwn, "Edit own articles", "Edit owned articles"),
        new("aaaaaaaa-0000-0000-0000-000000000009", AuthPermissions.Articles.Publish, "Publish articles", "Publish owned articles"),
        new("aaaaaaaa-0000-0000-0000-000000000010", AuthPermissions.Articles.Archive, "Archive articles", "Archive owned articles"),
        new("aaaaaaaa-0000-0000-0000-000000000011", AuthPermissions.Articles.DeleteOwn, "Delete own articles", "Delete owned articles"),
        new("aaaaaaaa-0000-0000-0000-000000000012", AuthPermissions.Analytics.GetOwn, "Own article analytics", "View analytics for owned articles"),
        new("aaaaaaaa-0000-0000-0000-000000000013", AuthPermissions.Users.Get, "Read users", "View users"),
        new("aaaaaaaa-0000-0000-0000-000000000014", AuthPermissions.Users.Create, "Create users", "Create users"),
        new("aaaaaaaa-0000-0000-0000-000000000015", AuthPermissions.Users.Update, "Update users", "Update users"),
        new("aaaaaaaa-0000-0000-0000-000000000016", AuthPermissions.Users.Delete, "Delete users", "Delete users"),
        new("aaaaaaaa-0000-0000-0000-000000000017", AuthPermissions.Users.AssignRoles, "Assign user roles", "Assign roles to users"),
        new("aaaaaaaa-0000-0000-0000-000000000018", AuthPermissions.Roles.Get, "Read roles", "View roles"),
        new("aaaaaaaa-0000-0000-0000-000000000019", AuthPermissions.Roles.Create, "Create roles", "Create roles"),
        new("aaaaaaaa-0000-0000-0000-000000000020", AuthPermissions.Roles.Update, "Update roles", "Update roles"),
        new("aaaaaaaa-0000-0000-0000-000000000021", AuthPermissions.Roles.Delete, "Delete roles", "Delete roles"),
        new("aaaaaaaa-0000-0000-0000-000000000022", AuthPermissions.Roles.AssignPermissions, "Assign role permissions", "Assign permissions to roles"),
        new("aaaaaaaa-0000-0000-0000-000000000023", AuthPermissions.Roles.AssignUsers, "Assign user roles", "Assign roles to users"),
        new("aaaaaaaa-0000-0000-0000-000000000024", AuthPermissions.PermissionsModule.Get, "Read permissions", "View permissions"),
        new("aaaaaaaa-0000-0000-0000-000000000025", AuthPermissions.PermissionsModule.Create, "Create permissions", "Create permissions"),
        new("aaaaaaaa-0000-0000-0000-000000000026", AuthPermissions.PermissionsModule.Update, "Update permissions", "Update permissions"),
        new("aaaaaaaa-0000-0000-0000-000000000027", AuthPermissions.PermissionsModule.Delete, "Delete permissions", "Delete permissions"),
        new("aaaaaaaa-0000-0000-0000-000000000028", AuthPermissions.Tags.Manage, "Manage tags", "Manage article tags"),
        new("aaaaaaaa-0000-0000-0000-000000000029", AuthPermissions.Articles.Moderate, "Moderate articles", "Moderate article content"),
        new("aaaaaaaa-0000-0000-0000-000000000030", AuthPermissions.Articles.DeleteAny, "Delete any article", "Delete any article"),
        new("aaaaaaaa-0000-0000-0000-000000000031", AuthPermissions.Analytics.GetSystem, "System analytics", "Access system analytics"),
        new("aaaaaaaa-0000-0000-0000-000000000032", AuthPermissions.Admin.ManageData, "Manage administrative data", "Manage administrative data")
    ];

    public static List<User> GeneratedUsers { get; private set; } = [];
    public static List<UserRole> GeneratedUserRoles { get; private set; } = [];
    public static List<Article> GeneratedArticles { get; private set; } = [];
    public static List<Tag> GeneratedTags { get; private set; } = [];
    public static List<ArticleTag> GeneratedArticleTags { get; private set; } = [];
    public static List<Comment> GeneratedComments { get; private set; } = [];
    public static List<Bookmark> GeneratedBookmarks { get; private set; } = [];
    public static List<Follow> GeneratedFollows { get; private set; } = [];
    public static List<ReadingHistory> GeneratedReadingHistories { get; private set; } = [];
    public static List<NewsLetterSubscription> GeneratedSubscriptions { get; private set; } = [];

    public static void Initialize(IPasswordHasher passwordHasher)
    {
        // Enforce exact global seed for Bogus to preserve mathematical determinism
        Randomizer.Seed = new Random(20260622);
        var faker = new Faker("en");

        var hashedPass = passwordHasher.HashPassword(DefaultPassword);

        // 1. Core Deterministic Administrative Users
        var adminUser = new User { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), Name = "Admin User", Email = "admin@medium.local", Password = hashedPass, Bio = "System Administrator" };
        var authorUser = new User { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), Name = "Author User", Email = "author@medium.local", Password = hashedPass, Bio = "Primary Tech Author" };
        var readerUser = new User { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), Name = "Reader User", Email = "reader@medium.local", Password = hashedPass, Bio = "Avid tech reader" };

        GeneratedUsers.AddRange([adminUser, authorUser, readerUser]);
        GeneratedUserRoles.AddRange([
            new UserRole { UserId = adminUser.Id, RoleId = AdminRoleId },
            new UserRole { UserId = authorUser.Id, RoleId = AuthorRoleId },
            new UserRole { UserId = readerUser.Id, RoleId = ReaderRoleId }
        ]);

        // 2. Generate 50 Deterministic Base Users (mix of Authors and Readers)
        var authorIds = new List<Guid> { authorUser.Id };
        var readerIds = new List<Guid> { readerUser.Id };

        for (int i = 1; i <= 50; i++)
        {
            var userId = Guid.Parse($"55555555-5555-5555-5555-{i:D12}");
            var isAuthor = i % 3 == 0; // Deterministic distribution rule

            var user = new User
            {
                Id = userId,
                Name = faker.Name.FullName(),
                Email = $"user{i:D3}@medium.local",
                Password = hashedPass,
                Bio = faker.Lorem.Sentence(8),
                AvatarUrl = faker.Internet.Avatar()
            };
            GeneratedUsers.Add(user);

            var intendedRole = isAuthor ? AuthorRoleId : ReaderRoleId;
            GeneratedUserRoles.Add(new UserRole { UserId = userId, RoleId = intendedRole });

            if (isAuthor) authorIds.Add(userId);
            else readerIds.Add(userId);
        }

        // 3. Generate Tags
        var staticTagNames = new[] { "dotnet", "csharp", "efcore", "webapi", "architecture", "sqlserver", "devops", "cloud", "security", "microservices" };
        for (int i = 0; i < staticTagNames.Length; i++)
        {
            GeneratedTags.Add(new Tag
            {
                Id = Guid.Parse($"66666666-6666-6666-6666-{i:D12}"),
                Name = staticTagNames[i],
                Slug = staticTagNames[i].ToLower()
            });
        }

        // 4. Generate Articles (authored only by valid Author IDs)
        int articleCounter = 1;
        ArticleStatus[] statuses = [ArticleStatus.Archived, ArticleStatus.Draft, ArticleStatus.Published, ArticleStatus.Scheduled];

        foreach (var authorId in authorIds)
        {
            for (int a = 0; a < 3; a++) // 3 articles per author
            {
                var artId = Guid.Parse($"77777777-7777-7777-7777-{articleCounter:D12}");
                var title = faker.Lorem.Sentence(5).TrimEnd('.');
                var status = statuses[(articleCounter) % statuses.Length];

                var article = new Article
                {
                    Id = artId,
                    AuthorId = authorId,
                    Title = title,
                    Slug = GenerateSlug(title),
                    Content = $"# {title}\n\n{faker.Lorem.Paragraphs(3)}",
                    CoverImageUrl = faker.Image.PicsumUrl(),
                    Status = status,
                    ViewCount = faker.Random.Long(10, 5000),
                    PublishedAt = status == ArticleStatus.Published ? DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30)) : null,
                    ScheduledAt = status == ArticleStatus.Scheduled ? DateTime.UtcNow.AddDays(faker.Random.Int(1, 5)) : null
                };

                GeneratedArticles.Add(article);

                // Pivot Junction: Associate 1-3 tags deterministically
                var tagIndices = Enumerable.Range(0, GeneratedTags.Count).OrderBy(x => (x + articleCounter) * 17 % 7).Take(faker.Random.Int(1, 3));
                foreach (var idx in tagIndices)
                {
                    GeneratedArticleTags.Add(new ArticleTag { ArticleId = artId, TagId = GeneratedTags[idx].Id });
                }

                articleCounter++;
            }
        }

        // Filter only published articles for relational actions (Comments, Bookmarks, Reading History)
        var publishedArticles = GeneratedArticles.Where(x => x.Status == ArticleStatus.Published).ToList();

        // 5. Generate Interaction Relations (Follows, Comments, Bookmarks, Reading History)
        int relationCounter = 1;
        foreach (var readerId in readerIds)
        {
            // Deterministic selection of targets based on reader context to maintain absolute state integrity
            var targetAuthors = authorIds.Where(authId => authId != readerId).OrderBy(x => x.GetHashCode() ^ readerId.GetHashCode()).Take(2).ToList();
            var targetArticles = publishedArticles.OrderBy(x => x.Id.GetHashCode() ^ readerId.GetHashCode()).Take(3).ToList();

            // Follows
            foreach (var authorId in targetAuthors)
            {
                GeneratedFollows.Add(new Follow
                {
                    Id = Guid.Parse($"88888888-8888-8888-8888-{relationCounter:D12}"),
                    FollowerId = readerId,
                    FollowingId = authorId
                });
                relationCounter++;
            }

            // Bookmarks & Comments & Reading Histories
            foreach (var article in targetArticles)
            {
                var interactionId = relationCounter;

                GeneratedBookmarks.Add(new Bookmark
                {
                    Id = Guid.Parse($"99999999-9999-9999-9999-{interactionId:D12}"),
                    UserId = readerId,
                    ArticleId = article.Id
                });

                GeneratedComments.Add(new Comment
                {
                    Id = Guid.Parse($"aaaaaaaa-bbbb-cccc-dddd-{interactionId:D12}"),
                    UserId = readerId,
                    ArticleId = article.Id,
                    Content = faker.Lorem.Sentence(6)
                });

                GeneratedReadingHistories.Add(new ReadingHistory
                {
                    Id = Guid.Parse($"bbbbbbbb-cccc-dddd-eeee-{interactionId:D12}"),
                    UserId = readerId,
                    ArticleId = article.Id,
                    DurationSeconds = faker.Random.Int(30, 600),
                    ReadAt = DateTime.UtcNow.AddHours(-faker.Random.Int(1, 72))
                });

                relationCounter++;
            }
        }

        // 6. Generate News Letter Subscriptions
        for (int i = 1; i <= 20; i++)
        {
            GeneratedSubscriptions.Add(new NewsLetterSubscription
            {
                Id = Guid.Parse($"cccccccc-dddd-eeee-ffff-{i:D12}"),
                Email = $"subscriber{i:D3}@example.com",
                IsActive = i % 5 != 0, // Deterministic status tracking
                SubscribedAt = DateTime.UtcNow.AddDays(-30)
            });
        }
    }

    public sealed record PermissionSeed(string IdValue, string Code, string Name, string Description)
    {
        public Guid Id { get; } = Guid.Parse(IdValue);
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}