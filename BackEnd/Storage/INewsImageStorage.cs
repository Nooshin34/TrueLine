namespace TrueLine.Api.Storage;

public interface INewsImageStorage
{
    Task<string> UploadAsync(
        int newsId,
        Stream content,
        long size,
        string contentType,
        CancellationToken cancellationToken);

    Task<StoredNewsImage?> OpenAsync(string objectKey, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed class StoredNewsImage
{
    public required Stream Content { get; init; }

    public required string ContentType { get; init; }
}
