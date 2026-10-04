using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence.Configurations;

public sealed class NewsImageConfiguration : IEntityTypeConfiguration<NewsImage>
{
    public void Configure(EntityTypeBuilder<NewsImage> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.ObjectKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(item => item.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(item => item.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(item => item.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(image => image.News)
            .WithMany(news => news.Images)
            .HasForeignKey(image => image.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(image => image.NewsId);
    }
}
