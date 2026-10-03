using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TrueLine.Domain.Enums;

namespace TrueLine.Domain.Dto;

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

    [Required]
    public NewsCategory Category { get; set; }

    public DateTime PublishedAt { get; set; }

    public bool IsPublished { get; set; }

    public IFormFile? Image { get; set; }
}
