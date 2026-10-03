namespace TrueLine.Application.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }

    bool IsAdmin { get; }

    bool IsReporter { get; }
}
