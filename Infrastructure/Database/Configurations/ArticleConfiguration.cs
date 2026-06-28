using Medium.Api.Enums;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
  public void Configure(EntityTypeBuilder<Article> builder)
  {
    builder.ToTable("articles");

    builder.Property(a => a.Title)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(a => a.Slug)
        .IsRequired()
        .HasMaxLength(255);

    builder.HasIndex(a => a.Slug).IsUnique();

    builder.Property(a => a.Content)
        .IsRequired();

    builder.Property(a => a.CoverImageUrl)
        .HasMaxLength(500);

    builder.Property(a => a.ThumbnailId)
        .IsRequired(false);

    builder.HasOne(a => a.Thumbnail)
        .WithMany()
        .HasForeignKey(a => a.ThumbnailId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(a => a.ContentImages)
        .WithOne(os => os.Article)
        .HasForeignKey(os => os.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Property(a => a.Status)
        .HasConversion(
            v => v.ToString(),
            v => (ArticleStatus)Enum.Parse(typeof(ArticleStatus), v));

    builder.Property(a => a.PublishedAt)
        .IsRequired(false);

    builder.Property(a => a.ScheduledAt)
        .IsRequired(false);

    builder.Property(a => a.ViewCount)
        .HasDefaultValue(0);

    builder.HasOne(a => a.Author)
        .WithMany(u => u.Articles)
        .HasForeignKey(a => a.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(a => a.ArticleTags)
        .WithOne(at => at.Article)
        .HasForeignKey(at => at.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(a => a.Comments)
        .WithOne(c => c.Article)
        .HasForeignKey(c => c.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(a => a.Bookmarks)
        .WithOne(b => b.Article)
        .HasForeignKey(b => b.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(a => a.ReadingHistories)
        .WithOne(rh => rh.Article)
        .HasForeignKey(rh => rh.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}