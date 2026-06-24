using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
  public void Configure(EntityTypeBuilder<Bookmark> builder)
  {
    builder.ToTable("bookmarks");

    builder.HasIndex(b => new { b.UserId, b.ArticleId }).IsUnique();

    builder.HasOne(b => b.User)
        .WithMany(u => u.Bookmarks)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(b => b.Article)
        .WithMany(a => a.Bookmarks)
        .HasForeignKey(b => b.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}