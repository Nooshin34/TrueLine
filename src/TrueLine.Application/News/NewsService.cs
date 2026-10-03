using TrueLine.Application.Abstractions;
using TrueLine.Application.Common;
using TrueLine.Domain.Entities;

namespace TrueLine.Application.Stories;

public interface INewsService
{
    Task<IReadOnlyList<News>> GetPublishedAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<News>>> GetMineAsync(CancellationToken cancellationToken);

    Task<ServiceResult<IReadOnlyList<News>>> GetReviewAsync(CancellationToken cancellationToken);

    Task<ServiceResult<News>> ApproveAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult<News>> UnapproveAsync(int id, CancellationToken cancellationToken);

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
    private readonly IReporterRepository _reporters;
    private readonly INewsImageStorage _images;
    private readonly ICurrentUser _current;
    private readonly IAdminAccess _admins;

    public NewsService(
        INewsRepository news,
        IReporterRepository reporters,
        INewsImageStorage images,
        ICurrentUser current,
        IAdminAccess admins)
    {
        _news = news;
        _reporters = reporters;
        _images = images;
        _current = current;
        _admins = admins;
    }

    public async Task<IReadOnlyList<News>> GetPublishedAsync(CancellationToken cancellationToken)
    {
        var news = await _news.ListPublishedAsync(cancellationToken);
        await AttachRatingsAsync(news, cancellationToken);
        return news;
    }

    public async Task<ServiceResult<IReadOnlyList<News>>> GetMineAsync(CancellationToken cancellationToken)
    {
        if (_current.Id is null || !_current.IsReporter)
        {
            return ServiceResult<IReadOnlyList<News>>.Fail(ServiceError.Unauthorized);
        }

        var news = await _news.ListByOwnerAsync(_current.Id.Value, cancellationToken);
        await AttachRatingsAsync(news, cancellationToken);
        return ServiceResult<IReadOnlyList<News>>.Ok(news);
    }

    public async Task<ServiceResult<IReadOnlyList<News>>> GetReviewAsync(CancellationToken cancellationToken)
    {
        if (!await _admins.IsCurrentAdminAsync(cancellationToken))
        {
            return ServiceResult<IReadOnlyList<News>>.Fail(ServiceError.Forbidden);
        }

        var news = await _news.ListSubmittedAsync(cancellationToken);
        await AttachRatingsAsync(news, cancellationToken);
        return ServiceResult<IReadOnlyList<News>>.Ok(news);
    }

    public async Task<ServiceResult<News>> ApproveAsync(int id, CancellationToken cancellationToken)
    {
        if (!await _admins.IsCurrentAdminAsync(cancellationToken))
        {
            return ServiceResult<News>.Fail(ServiceError.Forbidden);
        }

        var news = await _news.FindAsync(id, tracked: true, cancellationToken);
        if (news is null)
        {
            return ServiceResult<News>.Fail(ServiceError.NotFound);
        }

        if (!news.IsPublished)
        {
            return ServiceResult<News>.Fail(ServiceError.BadRequest, "Only a published story can be approved.");
        }

        news.IsApproved = true;
        await _news.SaveChangesAsync(cancellationToken);
        await AttachRatingsAsync([news], cancellationToken);
        return ServiceResult<News>.Ok(news);
    }

    public async Task<ServiceResult<News>> UnapproveAsync(int id, CancellationToken cancellationToken)
    {
        if (!await _admins.IsCurrentAdminAsync(cancellationToken))
        {
            return ServiceResult<News>.Fail(ServiceError.Forbidden);
        }

        var news = await _news.FindAsync(id, tracked: true, cancellationToken);
        if (news is null)
        {
            return ServiceResult<News>.Fail(ServiceError.NotFound);
        }

        news.IsApproved = false;
        await _news.SaveChangesAsync(cancellationToken);
        await AttachRatingsAsync([news], cancellationToken);
        return ServiceResult<News>.Ok(news);
    }

    public async Task<ServiceResult<News>> GetAsync(int id, CancellationToken cancellationToken)
    {
        var news = await _news.FindAsync(id, tracked: false, cancellationToken);
        if (news is null || !await CanReadAsync(news, cancellationToken))
        {
            return ServiceResult<News>.Fail(ServiceError.NotFound);
        }

        if (news.IsPublished && news.IsApproved)
        {
            news.ViewCount = await _news.IncrementViewCountAsync(id, cancellationToken);
        }

        await AttachRatingsAsync([news], cancellationToken);
        return ServiceResult<News>.Ok(news);
    }

    public async Task<ServiceResult<NewsFile>> OpenImageAsync(int id, CancellationToken cancellationToken)
    {
        var news = await _news.FindAsync(id, tracked: false, cancellationToken);
        if (news?.Image is null || !await CanReadAsync(news, cancellationToken))
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

            var reporterId = _current.IsReporter ? _current.Id : null;
            var author = reporterId is null ? null : await _reporters.FindNameAsync(reporterId.Value, cancellationToken);
            if (reporterId is null || string.IsNullOrWhiteSpace(author))
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
                IsApproved = false,
                ReporterId = reporterId,
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

            await AttachRatingsAsync([news], cancellationToken);
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
            var author = _current.IsReporter && _current.Id is not null
                ? await _reporters.FindNameAsync(_current.Id.Value, cancellationToken)
                : null;
            if (existing is null || !_current.IsReporter || existing.ReporterId != _current.Id || string.IsNullOrWhiteSpace(author))
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

            await AttachRatingsAsync([existing], cancellationToken);
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
        if (existing is null || !_current.IsReporter || existing.ReporterId != _current.Id)
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

    private async Task AttachRatingsAsync(IReadOnlyList<News> items, CancellationToken cancellationToken)
    {
        var reporterIds = items
            .Where(item => item.ReporterId is not null)
            .Select(item => item.ReporterId!.Value)
            .Distinct()
            .ToArray();
        var totals = await _news.SumViewsByReporterAsync(reporterIds, cancellationToken);

        foreach (var item in items)
        {
            var reads = item.ReporterId is int reporterId && totals.TryGetValue(reporterId, out var total) ? total : 0;
            item.AuthorStars = JournalistRating.Stars(reads);
        }
    }

    private async Task<bool> CanReadAsync(News news, CancellationToken cancellationToken)
    {
        if (news.IsPublished && news.IsApproved)
        {
            return true;
        }

        if (_current.IsReporter && news.ReporterId == _current.Id)
        {
            return true;
        }

        return news.IsPublished && await _admins.IsCurrentAdminAsync(cancellationToken);
    }
}
