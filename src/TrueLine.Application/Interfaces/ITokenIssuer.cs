namespace TrueLine.Application.Interfaces;

public interface ITokenIssuer
{
    string Create(int id, string email, string name, string role);
}
