using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrueLine.Api.Contracts;
using TrueLine.Application.Abstractions;
using TrueLine.Application.Stories;
using TrueLine.Domain.Entities;

namespace TrueLine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsService _news;
    private readonly ICurrentUser _current;

    public NewsController(INewsService news, ICurrentUser current)
    {
        _news = news;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetPublished(CancellationToken cancellationToken)
    {
        var news = await _news.GetPublishedAsync(cancellationToken);
        return Ok(news.Select(ToResponse));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _news.GetMineAsync(cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(result.Value.Select(ToResponse));
    }

    [Authorize]
    [HttpGet("review")]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetReview(CancellationToken cancellationToken)
    {
        var result = await _news.GetReviewAsync(cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(result.Value.Select(ToResponse));
    }

    [Authorize]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<NewsResponse>> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await _news.ApproveAsync(id, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [Authorize]
    [HttpPost("{id:int}/unapprove")]
    public async Task<ActionResult<NewsResponse>> Unapprove(int id, CancellationToken cancellationToken)
    {
        var result = await _news.UnapproveAsync(id, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NewsResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _news.GetAsync(id, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        var result = await _news.OpenImageAsync(id, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return File(result.Value.Content, result.Value.ContentType);
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 8 * 1024 * 1024)]
    public async Task<ActionResult<NewsResponse>> Create([FromForm] NewsWriteRequest request, CancellationToken cancellationToken)
    {
        var result = await _news.CreateAsync(ToDraft(request), ToFile(request.Image), cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, ToResponse(result.Value));
    }

    [Authorize]
    [HttpPut("{id:int}")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 8 * 1024 * 1024)]
    public async Task<ActionResult<NewsResponse>> Update(int id, [FromForm] NewsWriteRequest request, CancellationToken cancellationToken)
    {
        var result = await _news.UpdateAsync(id, ToDraft(request), ToFile(request.Image), cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(ToResponse(result.Value));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _news.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return this.ToActionResult(result);
        }

        return NoContent();
    }

    private NewsResponse ToResponse(News news) =>
        NewsResponses.From(news, _current, Request);

    private static NewsDraft ToDraft(NewsWriteRequest request) => new()
    {
        Title = request.Title,
        Summary = request.Summary,
        Body = request.Body,
        Category = request.Category,
        PublishedAt = request.PublishedAt,
        IsPublished = request.IsPublished,
    };

    private static IncomingFile? ToFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        return new IncomingFile
        {
            Content = file.OpenReadStream(),
            Length = file.Length,
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
    }
}
