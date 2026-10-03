using Microsoft.EntityFrameworkCore;
using TrueLine.Application.Abstractions;
using TrueLine.Domain.Entities;

namespace TrueLine.Infrastructure.Persistence;

public sealed class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;

    public AdminRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Admin?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Admins.FirstOrDefaultAsync(admin => admin.Email.ToLower() == email, cancellationToken);
}
