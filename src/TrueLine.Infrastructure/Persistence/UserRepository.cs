using Microsoft.EntityFrameworkCore;
using TrueLine.Application.Abstractions;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        _db.Users.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<string?> FindNameAsync(int id, CancellationToken cancellationToken) =>
        _db.Users
            .Where(user => user.Id == id)
            .Select(user => user.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(User user) => _db.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
