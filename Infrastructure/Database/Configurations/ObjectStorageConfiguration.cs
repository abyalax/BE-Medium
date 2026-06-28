using Medium.Api.Enums;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class ObjectStorageConfiguration : IEntityTypeConfiguration<ObjectStorage>
{
  public void Configure(EntityTypeBuilder<ObjectStorage> builder)
  {
    builder.ToTable("object_storages");

    builder.Property(a => a.Bucket)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(a => a.ObjectKey)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(a => a.MimeType)
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(a => a.OriginalName)
        .IsRequired()
        .HasMaxLength(150);

    builder.Property(a => a.Size)
        .IsRequired(false);

    builder.Property(a => a.ArticleId)
        .IsRequired(false);

    builder.Property(a => a.AccessTypes)
        .HasConversion(
            v => v.ToString(),
            v => (FileAccessType)Enum.Parse(typeof(FileAccessType), v));

    builder.HasOne(a => a.Author)
        .WithMany(u => u.ObjectStorages)
        .HasForeignKey(a => a.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(a => a.Article)
        .WithMany(a => a.ContentImages)
        .HasForeignKey(a => a.ArticleId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(a => a.ObjectKey).IsUnique();
  }
}