using TrueLine.Application.Abstractions;
using TrueLine.Application.Common;
using TrueLine.Domain.Entities;

namespace TrueLine.Application.Stories;

public interface INewsService
{
    Task<IReadOnlyList<News>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<News>>> GetMineAsync(CancellationToken cancellationToken);

    Task<ServiceResult<News>> GetAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<NewsFile>> OpenImageAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<News>> CreateAsync(NewsDraft draft, IncomingFile? image, CancellationToken cancellationToken);

    Task<ServiceResult<News>> UpdateAsync(int id, NewsDraft draft, IncomingFile? image, CancellationToken cancellationToken);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class NewsService : INewsService
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const string ImageStoreError =
        "Could not store the image. Check that MinIO is running and the MinIO settings are filled in.";
    private const string ImageTypeError = "Image must be a JPG, PNG, WEBP, or GIF no larger than 5 MB.";

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private readonly INewsRepository _news;
    private readonly IUserRepository _users;
    private readonly INewsImageStorage _images;
    private readonly ICurrentUser _current;

    public NewsService(
        INewsRepository news,
        IUserRepository users,
        INewsImageStorage images,
        ICurrentUser current)
    {
        _news = news;
        _users = users;
        _images = images;
        _current = current;
    }

    public Task<IReadOnlyList<News>> GetPublishedAsync(CancellationToken cancellationToken) =>
        _news.ListPublishedAsync(cancellationToken);

    public async Task<ServiceResult<IReadOnlyList<News>>> GetMineAsync(CancellationToken cancellationToken)
    {
        if (_current.Id is null)
        {
            return ServiceResult<IReadOnlyList<News>>.Fail(ServiceError.Unauthorized);
        }

        var news = await _news.ListByOwnerAsync(_current.Id.Value, cancellationToken);
        return ServiceResult<IReadOnlyList<News>>.Ok(news);
    }

    public async Task<ServiceResult<News>> GetAsync(int id, CancellationToken cancellationToken)
    {
        var news = await _news.FindAsync(id, tracked: false, cancellationToken);
        if (news is null || !CanRead(news))
        {
            return ServiceResult<News>.Fail(ServiceError.NotFound);
        }

        return ServiceResult<News>.Ok(news);
    }

    public async Task<ServiceResult<NewsFile>> OpenImageAsync(int id, CancellationToken cancellationToken)
    {
        var news = await _news.FindAsync(id, tracked: false, cancellationToken);
        if (news?.Image is null || !CanRead(news))
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        var stored = await _images.OpenAsync(news.Image.ObjectKey, cancellationToken);
        if (stored is null)
        {
            return ServiceResult<NewsFile>.Fail(ServiceError.NotFound);
        }

        return ServiceResult<NewsFile>.Ok(new NewsFile
        {
            Content = stored.Content,
            ContentType = news.Image.ContentType,
        });
    }

    public async Task<ServiceResult<News>> CreateAsync(
        NewsDraft draft,
        IncomingFile? image,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryValidateImage(image, out var imageError))
            {
                return ServiceResult<News>.Fail(ServiceError.BadRequest, imageError);
            }

            if (!Enum.IsDefined(draft.Category))
            {
                return ServiceResult<News>.Fail(ServiceError.BadRequest, "Choose a category.");
            }

            var userId = _current.Id;
            var author = userId is null ? null : await _users.FindNameAsync(userId.Value, cancellationToken);
            if (userId is null || string.IsNullOrWhiteSpace(author))
            {
                return ServiceResult<News>.Fail(ServiceError.Unauthorized);
            }

            var news = new News
            {
                Title = draft.Title.Trim(),
                Summary = string.IsNullOrWhiteSpace(draft.Summary) ? null : draft.Summary.Trim(),
                Body = draft.Body,
                Author = author,
                Category = draft.Category,
                PublishedAt = draft.PublishedAt == default ? DateTime.UtcNow : draft.PublishedAt,
                IsPublished = draft.IsPublished,
                UserId = userId,
            };

            _news.Add(news);
            await _news.SaveChangesAsync(cancellationToken);

            if (image is not null)
            {
                try
                {
                    await SaveImageAsync(news, image, cancellationToken);
                }
                catch (Exception)
                {
                    _news.Remove(news);
                    await _news.SaveChangesAsync(cancellationToken);
                    return ServiceResult<News>.Fail(ServiceError.StorageFailed, ImageStoreError);
                }
            }

            return ServiceResult<News>.Ok(news);
        }
        finally
        {
            if (image is not null)
            {
                await image.Content.DisposeAsync();
            }
        }
    }

    public async Task<ServiceResult<News>> UpdateAsync(
        int id,
        NewsDraft draft,
        IncomingFile? image,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TryValidateImage(image, out var imageError))
            {
                return ServiceResult<News>.Fail(ServiceError.BadRequest, imageError);
            }

            if (!Enum.IsDefined(draft.Category))
            {
                return ServiceResult<News>.Fail(ServiceError.BadRequest, "Choose a category.");
            }

            var existing = await _news.FindAsync(id, tracked: true, cancellationToken);
            var author = _current.Id is null ? null : await _users.FindNameAsync(_current.Id.Value, cancellationToken);
            if (existing is null || existing.UserId != _current.Id || string.IsNullOrWhiteSpace(author))
            {
                return ServiceResult<News>.Fail(ServiceError.NotFound);
            }

            existing.Title = draft.Title.Trim();
            existing.Summary = string.IsNullOrWhiteSpace(draft.Summary) ? null : draft.Summary.Trim();
            existing.Body = draft.Body;
            existing.Author = author;
            existing.Category = draft.Category;
            existing.IsPublished = draft.IsPublished;
            if (draft.PublishedAt != default)
            {
                existing.PublishedAt = draft.PublishedAt;
            }

            await _news.SaveChangesAsync(cancellationToken);

            if (image is not null)
            {
                try
                {
                    await ReplaceImageAsync(existing, image, cancellationToken);
                }
                catch (Exception)
                {
                    return ServiceResult<News>.Fail(ServiceError.StorageFailed, ImageStoreError);
                }
            }

            return ServiceResult<News>.Ok(existing);
        }
        finally
        {
            if (image is not null)
            {
                await image.Content.DisposeAsync();
            }
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await _news.FindAsync(id, tracked: true, cancellationToken);
        if (existing is null || existing.UserId != _current.Id)
        {
            return ServiceResult.Fail(ServiceError.NotFound);
        }

        if (existing.Image is not null)
        {
            try
            {
                await _images.DeleteAsync(existing.Image.ObjectKey, cancellationToken);
            }
            catch (Exception)
            {
            }
        }

        _news.Remove(existing);
        await _news.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    private async Task ReplaceImageAsync(News news, IncomingFile image, CancellationToken cancellationToken)
    {
        if (news.Image is not null)
        {
            var previousKey = news.Image.ObjectKey;
            _news.RemoveImage(news.Image);
            news.Image = null;
            await _news.SaveChangesAsync(cancellationToken);

            try
            {
                await _images.DeleteAsync(previousKey, cancellationToken);
            }
            catch (Exception)
            {
            }
        }

        await SaveImageAsync(news, image, cancellationToken);
    }

    private async Task SaveImageAsync(News news, IncomingFile image, CancellationToken cancellationToken)
    {
        var objectKey = await _images.UploadAsync(
            news.Id,
            image.Content,
            image.Length,
            image.ContentType,
            cancellationToken);

        news.Image = new NewsImage
        {
            NewsId = news.Id,
            ObjectKey = objectKey,
            ContentType = image.ContentType,
            OriginalFileName = Path.GetFileName(image.FileName),
        };

        _news.AddImage(news.Image);
        await _news.SaveChangesAsync(cancellationToken);
    }

    private static bool TryValidateImage(IncomingFile? image, out string? error)
    {
        error = null;
        if (image is null || image.Length == 0)
        {
            return true;
        }

        if (image.Length > MaxImageBytes || !AllowedImageTypes.Contains(image.ContentType))
        {
            error = ImageTypeError;
            return false;
        }

        return true;
    }

    private bool CanRead(News news) =>
        news.IsPublished || news.UserId == _current.Id;
}
