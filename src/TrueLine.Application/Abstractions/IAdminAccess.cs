namespace TrueLine.Application.Abstractions;

public interface IAdminAccess
{
    Task<bool> IsCurrentAdminAsync(CancellationToken cancellationToken);
}
