using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrueLine.Api.Contracts;
using TrueLine.Api.Data;
using TrueLine.Api.Storage;
using TrueLine.Backend.Entities;

namespace TrueLine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private readonly AppDbContext _db;
    private readonly INewsImageStorage _images;

    public NewsController(AppDbContext db, INewsImageStorage images)
    {
        _db = db;
        _images = images;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetPublished(CancellationToken cancellationToken)
    {
        var news = await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.IsPublished)
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);

        return Ok(news.Select(ToResponse));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var news = await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync(cancellationToken);

        return Ok(news.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NewsResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var news = await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (news is null || !CanRead(news))
        {
            return NotFound();
        }

        return Ok(ToResponse(news));
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        var news = await _db.News
            .AsNoTracking()
            .Include(item => item.Image)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (news?.Image is null || !CanRead(news))
        {
            return NotFound();
        }

        var stored = await _images.OpenAsync(news.Image.ObjectKey, cancellationToken);
        if (stored is null)
        {
            return NotFound();
        }

        return File(stored.Content, news.Image.ContentType);
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 8 * 1024 * 1024)]
    public async Task<ActionResult<NewsResponse>> Create([FromForm] NewsWriteRequest request, CancellationToken cancellationToken)
    {
        if (!TryValidateImage(request.Image, out var imageError))
        {
            return BadRequest(imageError);
        }

        if (!Enum.IsDefined(request.Category))
        {
            return BadRequest("Choose a category.");
        }

        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var news = new News
        {
            Title = request.Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim(),
            Body = request.Body,
            Author = request.Author.Trim(),
            Category = request.Category,
            PublishedAt = request.PublishedAt == default ? DateTime.UtcNow : request.PublishedAt,
            IsPublished = request.IsPublished,
            UserId = userId,
        };

        _db.News.Add(news);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.Image is { Length: > 0 })
        {
            try
            {
                await SaveImageAsync(news, request.Image, cancellationToken);
            }
            catch (Exception)
            {
                _db.News.Remove(news);
                await _db.SaveChangesAsync(cancellationToken);
                return Problem(
                    detail: "Could not store the image. Check that MinIO is running and the MinIO settings are filled in.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }

        return CreatedAtAction(nameof(GetById), new { id = news.Id }, ToResponse(news));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 8 * 1024 * 1024)]
    public async Task<ActionResult<NewsResponse>> Update(int id, [FromForm] NewsWriteRequest request, CancellationToken cancellationToken)
    {
        if (!TryValidateImage(request.Image, out var imageError))
        {
            return BadRequest(imageError);
        }

        if (!Enum.IsDefined(request.Category))
        {
            return BadRequest("Choose a category.");
        }

        var existing = await _db.News
            .Include(item => item.Image)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (existing is null || existing.UserId != CurrentUserId())
        {
            return NotFound();
        }

        existing.Title = request.Title.Trim();
        existing.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        existing.Body = request.Body;
        existing.Author = request.Author.Trim();
        existing.Category = request.Category;
        existing.IsPublished = request.IsPublished;
        if (request.PublishedAt != default)
        {
            existing.PublishedAt = request.PublishedAt;
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (request.Image is { Length: > 0 })
        {
            try
            {
                await ReplaceImageAsync(existing, request.Image, cancellationToken);
            }
            catch (Exception)
            {
                return Problem(
                    detail: "Could not store the image. Check that MinIO is running and the MinIO settings are filled in.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }

        return Ok(ToResponse(existing));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await _db.News
            .Include(item => item.Image)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (existing is null || existing.UserId != CurrentUserId())
        {
            return NotFound();
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

        _db.News.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task ReplaceImageAsync(News news, IFormFile image, CancellationToken cancellationToken)
    {
        if (news.Image is not null)
        {
            var previousKey = news.Image.ObjectKey;
            _db.NewsImages.Remove(news.Image);
            news.Image = null;
            await _db.SaveChangesAsync(cancellationToken);

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

    private async Task SaveImageAsync(News news, IFormFile image, CancellationToken cancellationToken)
    {
        await using var stream = image.OpenReadStream();
        var objectKey = await _images.UploadAsync(
            news.Id,
            stream,
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

        _db.NewsImages.Add(news.Image);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static bool TryValidateImage(IFormFile? image, out string? error)
    {
        error = null;
        if (image is null || image.Length == 0)
        {
            return true;
        }

        if (image.Length > MaxImageBytes || !AllowedImageTypes.Contains(image.ContentType))
        {
            error = "Image must be a JPG, PNG, WEBP, or GIF no larger than 5 MB.";
            return false;
        }

        return true;
    }

    private bool CanRead(News news) =>
        news.IsPublished || news.UserId == CurrentUserId();

    private int? CurrentUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var id) ? id : null;
    }

    private NewsResponse ToResponse(News news) => new()
    {
        Id = news.Id,
        Title = news.Title,
        Summary = news.Summary,
        Body = news.Body,
        Author = news.Author,
        Category = news.Category,
        PublishedAt = news.PublishedAt,
        IsPublished = news.IsPublished,
        UserId = news.UserId,
        CanEdit = news.UserId is not null && news.UserId == CurrentUserId(),
        ImageUrl = news.Image is null
            ? null
            : $"{Request.Scheme}://{Request.Host}/api/news/{news.Id}/image",
    };
}
