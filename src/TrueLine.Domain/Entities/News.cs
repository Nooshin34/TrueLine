namespace TrueLine.Domain.Entities;

public class News
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public NewsCategory Category { get; set; } = NewsCategory.World;

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublished { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public NewsImage? Image { get; set; }
}
