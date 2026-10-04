using Microsoft.EntityFrameworkCore;
using TrueLine.Domain.Entities;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
