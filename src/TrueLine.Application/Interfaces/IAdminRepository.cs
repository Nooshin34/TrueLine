using TrueLine.Domain.Entities;

namespace TrueLine.Application.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}
