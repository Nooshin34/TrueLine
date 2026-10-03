using Microsoft.EntityFrameworkCore;
using TrueLine.Application.Interfaces;
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
            .Where(item => item.IsPublished && item.IsApproved)
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<News>> ListSubmittedAsync(CancellationToken cancellationToken)
    {
        return await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.IsPublished)
            .OrderBy(item => item.IsApproved)
            .ThenByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<News>> ListByOwnerAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.ReporterId == userId)
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

    public async Task<int> IncrementViewCountAsync(int id, CancellationToken cancellationToken)
    {
        await _db.News
            .Where(item => item.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.ViewCount, item => item.ViewCount + 1),
                cancellationToken);

        return await _db.News
            .Where(item => item.Id == id)
            .Select(item => item.ViewCount)
            .FirstAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, int>> SumViewsByReporterAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<int, int>();
        }

        return await _db.News
            .Where(item => item.ReporterId != null && userIds.Contains(item.ReporterId.Value))
            .GroupBy(item => item.ReporterId!.Value)
            .Select(group => new { UserId = group.Key, Views = group.Sum(item => item.ViewCount) })
            .ToDictionaryAsync(group => group.UserId, group => group.Views, cancellationToken);
    }

    public void Add(News news) => _db.News.Add(news);

    public void Remove(News news) => _db.News.Remove(news);

    public void AddImage(NewsImage image) => _db.NewsImages.Add(image);

    public void RemoveImage(NewsImage image) => _db.NewsImages.Remove(image);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
