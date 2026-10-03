using Microsoft.EntityFrameworkCore;
using TrueLine.Application.Interfaces;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence;

public sealed class ReporterRepository : IReporterRepository
{
    private readonly AppDbContext _db;

    public ReporterRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        _db.Reporters.AnyAsync(reporter => reporter.Email == email, cancellationToken);

    public Task<Reporter?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Reporters.FirstOrDefaultAsync(reporter => reporter.Email == email, cancellationToken);

    public Task<Reporter?> FindByIdAsync(int id, CancellationToken cancellationToken) =>
        _db.Reporters.FirstOrDefaultAsync(reporter => reporter.Id == id, cancellationToken);

    public Task<string?> FindNameAsync(int id, CancellationToken cancellationToken) =>
        _db.Reporters
            .Where(reporter => reporter.Id == id)
            .Select(reporter => reporter.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(Reporter reporter) => _db.Reporters.Add(reporter);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
