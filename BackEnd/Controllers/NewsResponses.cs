using TrueLine.Application.Interfaces;
using TrueLine.Domain.Dto;
using TrueLine.Domain.Entities;

namespace TrueLine.Api.Controllers;

internal static class NewsResponses
{
    public static NewsResponse From(News news, ICurrentUser current, HttpRequest request)
    {
        var images = news.Images
            .OrderBy(image => image.SortOrder)
            .ThenBy(image => image.Id)
            .Select(image => new NewsImageLink
            {
                Id = image.Id,
                Url = $"{request.Scheme}://{request.Host}/api/news/{news.Id}/images/{image.Id}",
            })
            .ToList();

        return new NewsResponse
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
            ImageUrl = images.Count == 0 ? null : images[0].Url,
            Images = images,
        };
    }
}
