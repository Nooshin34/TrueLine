using Microsoft.EntityFrameworkCore;
using TrueLine.Domain.Entities;
using TrueLine.Domain.Enums;

namespace TrueLine.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<News> News { get; set; }

    public DbSet<NewsImage> NewsImages { get; set; }

    public DbSet<Reporter> Reporters { get; set; }

    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>(news =>
        {
            news.HasKey(item => item.Id);

            news.Property(item => item.Title)
                .IsRequired()
                .HasMaxLength(200);

            news.Property(item => item.Summary)
                .HasMaxLength(500);

            news.Property(item => item.Body)
                .IsRequired();

            news.Property(item => item.Author)
                .IsRequired()
                .HasMaxLength(100);

            news.Property(item => item.Category)
                .IsRequired()
                .HasDefaultValue(NewsCategory.World);

            news.Property(item => item.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            news.Property(item => item.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            news.Ignore(item => item.AuthorStars);

            news.HasOne(item => item.Reporter)
                .WithMany()
                .HasForeignKey(item => item.ReporterId)
                .OnDelete(DeleteBehavior.SetNull);

            news.HasIndex(item => item.ReporterId);

            news.HasOne(item => item.Image)
                .WithOne(image => image.News)
                .HasForeignKey<NewsImage>(image => image.NewsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NewsImage>(image =>
        {
            image.HasKey(item => item.Id);

            image.Property(item => item.ObjectKey)
                .IsRequired()
                .HasMaxLength(500);

            image.Property(item => item.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            image.Property(item => item.OriginalFileName)
                .IsRequired()
                .HasMaxLength(260);

            image.HasIndex(item => item.NewsId)
                .IsUnique();
        });

        modelBuilder.Entity<Reporter>(reporter =>
        {
            reporter.ToTable("Reporters");

            reporter.HasKey(item => item.Id);

            reporter.Property(item => item.Name)
                .IsRequired()
                .HasMaxLength(100);

            reporter.Property(item => item.Email)
                .IsRequired()
                .HasMaxLength(256);

            reporter.HasIndex(item => item.Email)
                .IsUnique();

            reporter.Property(item => item.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            reporter.Property(item => item.AvatarObjectKey)
                .HasMaxLength(500);

            reporter.Property(item => item.AvatarContentType)
                .HasMaxLength(100);

            reporter.Property(item => item.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<Admin>(admin =>
        {
            admin.ToTable("Admins");

            admin.HasKey(item => item.Id);

            admin.Property(item => item.Name)
                .IsRequired()
                .HasMaxLength(100);

            admin.Property(item => item.Email)
                .IsRequired()
                .HasMaxLength(256);

            admin.HasIndex(item => item.Email)
                .IsUnique();

            admin.Property(item => item.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            admin.Property(item => item.CreatedAt)
                .IsRequired();
        });
    }
}
