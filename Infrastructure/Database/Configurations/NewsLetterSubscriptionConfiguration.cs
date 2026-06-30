using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class NewsLetterSubscriptionConfiguration : IEntityTypeConfiguration<NewsLetterSubscription>
{
  public void Configure(EntityTypeBuilder<NewsLetterSubscription> builder)
  {
    builder.ToTable("news_letter_subscriptions");

    builder.Property(nls => nls.Email)
      .IsRequired()
      .HasMaxLength(255);

    builder.HasIndex(nls => nls.Email).IsUnique();

    builder.Property(nls => nls.IsActive)
      .IsRequired()
      .HasDefaultValue(true);

    builder.Property(nls => nls.SubscribedAt)
      .IsRequired(false);

    builder.Property(nls => nls.UnsubscribedAt)
      .IsRequired(false);
  }
}