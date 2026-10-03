using TrueLine.Domain.Entities;

namespace TrueLine.Application.Abstractions;

public interface IAdminRepository
{
    Task<Admin?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
