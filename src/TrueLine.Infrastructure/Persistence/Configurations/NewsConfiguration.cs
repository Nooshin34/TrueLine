using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrueLine.Domain.Entities;
using TrueLine.Domain.Enums;

namespace TrueLine.Infrastructure.Persistence.Configurations;

public sealed class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(item => item.Summary)
            .HasMaxLength(500);

        builder.Property(item => item.Body)
            .IsRequired();

        builder.Property(item => item.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(item => item.Category)
            .IsRequired()
            .HasDefaultValue(NewsCategory.World);

        builder.Property(item => item.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(item => item.ViewCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Ignore(item => item.AuthorStars);

        builder.HasOne(item => item.Reporter)
            .WithMany()
            .HasForeignKey(item => item.ReporterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(item => item.ReporterId);
    }
}
