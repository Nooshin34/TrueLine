namespace TrueLine.Backend.Entities;

public class NewsImage
{
    public int Id { get; set; }

    public int NewsId { get; set; }

    public News News { get; set; } = null!;

    public string ObjectKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
}
