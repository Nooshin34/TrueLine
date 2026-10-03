namespace TrueLine.Application.Abstractions;

public interface ITokenIssuer
{
    string Create(int id, string email, string name, string role);
}
