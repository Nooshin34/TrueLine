using Microsoft.EntityFrameworkCore;
using TrueLine.Backend.Entities;

namespace TrueLine.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<News> News { get; set; }

    public DbSet<NewsImage> NewsImages { get; set; }

    public DbSet<User> Users { get; set; }

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

            news.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            news.HasIndex(item => item.UserId);

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

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(item => item.Id);

            user.Property(item => item.Name)
                .IsRequired()
                .HasMaxLength(100);

            user.Property(item => item.Email)
                .IsRequired()
                .HasMaxLength(256);

            user.HasIndex(item => item.Email)
                .IsUnique();

            user.Property(item => item.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            user.Property(item => item.CreatedAt)
                .IsRequired();
        });
    }
}
