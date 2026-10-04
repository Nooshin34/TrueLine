using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence.Configurations;

public sealed class ReporterConfiguration : IEntityTypeConfiguration<Reporter>
{
    public void Configure(EntityTypeBuilder<Reporter> builder)
    {
        builder.ToTable("Reporters");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(item => item.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(item => item.Email)
            .IsUnique();

        builder.Property(item => item.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(item => item.AvatarObjectKey)
            .HasMaxLength(500);

        builder.Property(item => item.AvatarContentType)
            .HasMaxLength(100);

        builder.Property(item => item.CreatedAt)
            .IsRequired();
    }
}
