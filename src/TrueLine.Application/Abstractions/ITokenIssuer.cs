using TrueLine.Domain.Entities;

namespace TrueLine.Application.Abstractions;

public interface ITokenIssuer
{
    string Create(User user);
}
