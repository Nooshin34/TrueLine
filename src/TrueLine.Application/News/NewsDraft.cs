using TrueLine.Domain.Entities;

namespace TrueLine.Application.Stories;

public sealed class NewsDraft
{
    public required string Title { get; init; }

    public string? Summary { get; init; }

    public required string Body { get; init; }

    public required NewsCategory Category { get; init; }

    public DateTime PublishedAt { get; init; }

    public bool IsPublished { get; init; }
}

public sealed class IncomingFile
{
    public required Stream Content { get; init; }

    public required long Length { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}

public sealed class NewsFile
{
    public required Stream Content { get; init; }

    public required string ContentType { get; init; }
}
