using HackerNewsBestStories.Api.Models;
using HackerNewsBestStories.Api.Services;

namespace HackerNewsBestStories.Tests;

internal sealed class FakeHackerNewsClient : IHackerNewsClient
{
    private readonly Dictionary<int, HackerNewsItem?> _stories = new();
    private readonly HashSet<int> _failingIds = new();
    private int _bestStoryIdsCallCount;

    public int BestStoryIdsCallCount => _bestStoryIdsCallCount;

    public FakeHackerNewsClient WithStory(int id, int score)
    {
        _stories[id] = new HackerNewsItem(
            Id: id,
            Title: $"Story {id}",
            Url: $"https://example.com/{id}",
            By: "author",
            Time: 1570887781,
            Score: score,
            Descendants: 10,
            Type: "story");
        return this;
    }

    public FakeHackerNewsClient WithDeletedStory(int id)
    {
        _stories[id] = null;
        return this;
    }

    public FakeHackerNewsClient WithFailingStory(int id)
    {
        _failingIds.Add(id);
        return this;
    }

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _bestStoryIdsCallCount);
        await Task.Delay(50, cancellationToken);
        return [.. _stories.Keys, .. _failingIds];
    }

    public Task<HackerNewsItem?> GetStoryAsync(int storyId, CancellationToken cancellationToken)
    {
        if (_failingIds.Contains(storyId))
        {
            return Task.FromException<HackerNewsItem?>(new HttpRequestException("Simulated failure"));
        }

        return Task.FromResult(_stories.GetValueOrDefault(storyId));
    }
}