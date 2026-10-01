using HackerNewsBestStories.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace HackerNewsBestStories.Tests;

public sealed class BestStoriesServiceTests
{
    private static BestStoriesService CreateService(FakeHackerNewsClient client) =>
        new(client, new MemoryCache(new MemoryCacheOptions()), NullLogger<BestStoriesService>.Instance);

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsStoriesOrderedByScoreDescending()
    {
        var client = new FakeHackerNewsClient()
            .WithStory(id: 1, score: 50)
            .WithStory(id: 2, score: 300)
            .WithStory(id: 3, score: 120);
        var service = CreateService(client);

        var stories = await service.GetBestStoriesAsync(3, CancellationToken.None);

        Assert.Equal(new[] { 300, 120, 50 }, stories.Select(s => s.Score));
    }

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsOnlyRequestedCount()
    {
        var client = new FakeHackerNewsClient()
            .WithStory(id: 1, score: 50)
            .WithStory(id: 2, score: 300)
            .WithStory(id: 3, score: 120);
        var service = CreateService(client);

        var stories = await service.GetBestStoriesAsync(2, CancellationToken.None);

        Assert.Equal(new[] { 300, 120 }, stories.Select(s => s.Score));
    }

    [Fact]
    public async Task GetBestStoriesAsync_WhenCountExceedsAvailable_ReturnsAllStories()
    {
        var client = new FakeHackerNewsClient()
            .WithStory(id: 1, score: 50)
            .WithStory(id: 2, score: 300);
        var service = CreateService(client);

        var stories = await service.GetBestStoriesAsync(100, CancellationToken.None);

        Assert.Equal(2, stories.Count);
    }

    [Fact]
    public async Task GetBestStoriesAsync_SkipsStoriesThatFailToLoad()
    {
        var client = new FakeHackerNewsClient()
            .WithStory(id: 1, score: 50)
            .WithFailingStory(id: 2);
        var service = CreateService(client);

        var stories = await service.GetBestStoriesAsync(10, CancellationToken.None);

        var story = Assert.Single(stories);
        Assert.Equal(50, story.Score);
    }

    [Fact]
    public async Task GetBestStoriesAsync_SkipsDeletedStories()
    {
        var client = new FakeHackerNewsClient()
            .WithStory(id: 1, score: 50)
            .WithDeletedStory(id: 2);
        var service = CreateService(client);

        var stories = await service.GetBestStoriesAsync(10, CancellationToken.None);

        Assert.Single(stories);
    }

    [Fact]
    public async Task GetBestStoriesAsync_SecondCall_IsServedFromCache()
    {
        var client = new FakeHackerNewsClient().WithStory(id: 1, score: 50);
        var service = CreateService(client);

        await service.GetBestStoriesAsync(1, CancellationToken.None);
        await service.GetBestStoriesAsync(1, CancellationToken.None);

        Assert.Equal(1, client.BestStoryIdsCallCount);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ConcurrentCalls_FetchFromHackerNewsOnlyOnce()
    {
        var client = new FakeHackerNewsClient().WithStory(id: 1, score: 50);
        var service = CreateService(client);

        var calls = Enumerable.Range(0, 20)
            .Select(_ => service.GetBestStoriesAsync(1, CancellationToken.None));
        await Task.WhenAll(calls);

        Assert.Equal(1, client.BestStoryIdsCallCount);
    }
}