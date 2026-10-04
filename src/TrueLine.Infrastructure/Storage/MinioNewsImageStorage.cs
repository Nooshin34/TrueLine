using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using TrueLine.Application.Interfaces;

namespace TrueLine.Infrastructure.Storage;

public class MinioNewsImageStorage : INewsImageStorage
{
    private readonly IMinioClient _minio;
    private readonly MinioOptions _options;

    public MinioNewsImageStorage(IMinioClient minio, IOptions<MinioOptions> options)
    {
        _minio = minio;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        int newsId,
        Stream content,
        long size,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = ExtensionFor(contentType);
        var objectKey = $"news/{newsId}/{Guid.NewGuid():N}{extension}";
        await SaveObjectAsync(objectKey, content, contentType, cancellationToken);
        return objectKey;
    }

    public Task UploadObjectAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        return SaveObjectAsync(objectKey, content, contentType, cancellationToken);
    }

    public async Task<StoredObject?> OpenAsync(string objectKey, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var memory = new MemoryStream();

        try
        {
            await _minio.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(objectKey)
                    .WithCallbackStream(stream => stream.CopyTo(memory)),
                cancellationToken);
        }
        catch (Exception)
        {
            await memory.DisposeAsync();
            return null;
        }

        memory.Position = 0;
        return new StoredObject
        {
            Content = memory,
            ContentType = "application/octet-stream",
        };
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return;
        }

        EnsureConfigured();

        try
        {
            await _minio.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(objectKey),
                cancellationToken);
        }
        catch (ObjectNotFoundException)
        {
        }
    }

    private async Task SaveObjectAsync(
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        await EnsureBucketAsync(cancellationToken);

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(objectKey)
                .WithStreamData(buffer)
                .WithObjectSize(buffer.Length)
                .WithContentType(contentType),
            cancellationToken);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var exists = await _minio.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.Bucket),
            cancellationToken);

        if (!exists)
        {
            await _minio.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.Bucket),
                cancellationToken);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.SecretKey)
            || string.IsNullOrWhiteSpace(_options.Bucket))
        {
            throw new InvalidOperationException("MinIO settings are missing.");
        }
    }

    private static string ExtensionFor(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => ".bin",
    };
}
