using TrueLine.Application.Interfaces;

namespace TrueLine.Application.Services;

public sealed class AdminAccess : IAdminAccess
{
    private readonly ICurrentUser _current;

    public AdminAccess(ICurrentUser current)
    {
        _current = current;
    }

    public Task<bool> IsCurrentAdminAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_current.IsAdmin);
}
