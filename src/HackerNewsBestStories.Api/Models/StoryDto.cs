namespace HackerNewsBestStories.Api.Models;

public sealed record StoryDto(
    string Title,
    string? Uri,
    string PostedBy,
    DateTimeOffset Time,
    int Score,
    int CommentCount)
{
    public static StoryDto FromHackerNewsItem(HackerNewsItem item) => new(
        Title: item.Title ?? string.Empty,
        Uri: item.Url,
        PostedBy: item.By ?? string.Empty,
        Time: DateTimeOffset.FromUnixTimeSeconds(item.Time),
        Score: item.Score,
        CommentCount: item.Descendants);
}
