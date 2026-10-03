using TrueLine.Domain.Entities;

namespace TrueLine.Application.Interfaces;

public interface IReporterRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<Reporter?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Reporter?> FindByIdAsync(int id, CancellationToken cancellationToken);

    Task<string?> FindNameAsync(int id, CancellationToken cancellationToken);

    void Add(Reporter reporter);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
