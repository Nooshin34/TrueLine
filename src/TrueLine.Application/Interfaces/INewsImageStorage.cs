namespace TrueLine.Application.Interfaces;

public interface INewsImageStorage
{
    Task<string> UploadAsync(
        int newsId,
        Stream content,
        long size,
        string contentType,
        CancellationToken cancellationToken);

    Task UploadObjectAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task<StoredObject?> OpenAsync(string objectKey, CancellationToken cancellationToken);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken);
}

public sealed class StoredObject
{
    public required Stream Content { get; init; }

    public required string ContentType { get; init; }
}
