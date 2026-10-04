using TrueLine.Domain.Enums;

namespace TrueLine.Domain.Dto;

public class NewsResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public NewsCategory Category { get; set; }

    public DateTime PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public bool IsApproved { get; set; }

    public int ViewCount { get; set; }

    public int AuthorStars { get; set; }

    public int? ReporterId { get; set; }

    public bool CanEdit { get; set; }

    public string? ImageUrl { get; set; }

    public IReadOnlyList<NewsImageLink> Images { get; set; } = [];
}

public class NewsImageLink
{
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;
}
