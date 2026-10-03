using TrueLine.Api.Contracts;
using TrueLine.Application.Abstractions;
using TrueLine.Domain.Entities;

namespace TrueLine.Api.Controllers;

internal static class NewsResponses
{
    public static NewsResponse From(News news, ICurrentUser current, HttpRequest request) => new()
    {
        Id = news.Id,
        Title = news.Title,
        Summary = news.Summary,
        Body = news.Body,
        Author = news.Author,
        Category = news.Category,
        PublishedAt = news.PublishedAt,
        IsPublished = news.IsPublished,
        IsApproved = news.IsApproved,
        ViewCount = news.ViewCount,
        AuthorStars = news.AuthorStars,
        ReporterId = news.ReporterId,
        CanEdit = current.IsReporter && news.ReporterId is not null && news.ReporterId == current.Id,
        ImageUrl = news.Image is null
            ? null
            : $"{request.Scheme}://{request.Host}/api/news/{news.Id}/image",
    };
}
