using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TrueLine.Api.Contracts;

public class NewsWriteRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public IFormFile? Image { get; set; }
}

public class NewsResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public string? ImageUrl { get; set; }
}
