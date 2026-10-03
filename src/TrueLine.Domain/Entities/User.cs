namespace TrueLine.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarObjectKey { get; set; }

    public string? AvatarContentType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
