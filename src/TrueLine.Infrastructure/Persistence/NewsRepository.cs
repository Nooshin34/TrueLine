using Microsoft.EntityFrameworkCore;
using TrueLine.Application.Abstractions;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence;

public sealed class NewsRepository : INewsRepository
{
    private readonly AppDbContext _db;

    public NewsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<News>> ListPublishedAsync(CancellationToken cancellationToken)
    {
        return await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.IsPublished)
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<News>> ListByOwnerAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<News?> FindAsync(int id, bool tracked, CancellationToken cancellationToken)
    {
        var query = _db.News.Include(item => item.Image).AsQueryable();
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public void Add(News news) => _db.News.Add(news);

    public void Remove(News news) => _db.News.Remove(news);

    public void AddImage(NewsImage image) => _db.NewsImages.Add(image);

    public void RemoveImage(NewsImage image) => _db.NewsImages.Remove(image);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
