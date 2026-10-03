namespace TrueLine.Application.Interfaces;

public interface IAdminAccess
{
    Task<bool> IsCurrentAdminAsync(CancellationToken cancellationToken);
}
