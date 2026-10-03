using TrueLine.Domain.Entities;

namespace TrueLine.Application.Abstractions;

public interface INewsRepository
{
    Task<IReadOnlyList<News>> ListPublishedAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<News>> ListSubmittedAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<News>> ListByOwnerAsync(int userId, CancellationToken cancellationToken);

    Task<News?> FindAsync(int id, bool tracked, CancellationToken cancellationToken);

    Task<int> IncrementViewCountAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, int>> SumViewsByReporterAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken);

    void Add(News news);

    void Remove(News news);

    void AddImage(NewsImage image);

    void RemoveImage(NewsImage image);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
