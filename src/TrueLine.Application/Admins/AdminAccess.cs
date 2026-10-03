using TrueLine.Application.Abstractions;

namespace TrueLine.Application.Admins;

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
