using HackerNewsBestStories.Api.Models;

namespace HackerNewsBestStories.Tests;

public sealed class StoryDtoTests
{
    [Fact]
    public void FromHackerNewsItem_MapsAllFields()
    {
        var item = new HackerNewsItem(
            Id: 21233041,
            Title: "A uBlock Origin update was rejected from the Chrome Web Store",
            Url: "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
            By: "ismaildonmez",
            Time: 1570887781,
            Score: 1716,
            Descendants: 572,
            Type: "story");

        var dto = StoryDto.FromHackerNewsItem(item);

        Assert.Equal(item.Title, dto.Title);
        Assert.Equal(item.Url, dto.Uri);
        Assert.Equal("ismaildonmez", dto.PostedBy);
        Assert.Equal(new DateTimeOffset(2019, 10, 12, 13, 43, 1, TimeSpan.Zero), dto.Time);
        Assert.Equal(1716, dto.Score);
        Assert.Equal(572, dto.CommentCount);
    }

    [Fact]
    public void FromHackerNewsItem_WhenFieldsMissing_UsesSafeDefaults()
    {
        var item = new HackerNewsItem(1, null, null, null, 0, 0, 0, null);

        var dto = StoryDto.FromHackerNewsItem(item);

        Assert.Equal(string.Empty, dto.Title);
        Assert.Null(dto.Uri);
        Assert.Equal(string.Empty, dto.PostedBy);
    }
}