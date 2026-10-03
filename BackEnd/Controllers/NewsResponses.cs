using TrueLine.Api.Contracts;
using TrueLine.Domain.Entities;

namespace TrueLine.Api.Controllers;

internal static class NewsResponses
{
    public static NewsResponse From(News news, int? currentUserId, HttpRequest request) => new()
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
        CanEdit = news.UserId is not null && news.UserId == currentUserId,
        ImageUrl = news.Image is null
            ? null
            : $"{request.Scheme}://{request.Host}/api/news/{news.Id}/image",
    };
}
