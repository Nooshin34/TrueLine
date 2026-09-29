using System.ComponentModel.DataAnnotations;

namespace TrueLine.Backend.Entities;

public class NewsImage
{
    public int Id { get; set; }

    public int NewsId { get; set; }

    public News News { get; set; } = null!;

    [MaxLength(500)]
    public string ObjectKey { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;
}
