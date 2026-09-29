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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>()
            .HasOne(news => news.Image)
            .WithOne(image => image.News)
            .HasForeignKey<NewsImage>(image => image.NewsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NewsImage>()
            .HasIndex(image => image.NewsId)
            .IsUnique();
    }
}
