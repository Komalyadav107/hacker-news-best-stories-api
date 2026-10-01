using System.Collections.Concurrent;
using HackerNewsBestStories.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsBestStories.Api.Services;

public sealed class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<BestStoriesService> _logger;
    private const string CacheKey = "best-stories";
    private const int MaxConcurrentRequests = 10;
    private readonly SemaphoreSlim _refreshLock = new(1,1);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public BestStoriesService(IHackerNewsClient hackerNewsClient, IMemoryCache memoryCache, ILogger<BestStoriesService> logger  )
    {
        _hackerNewsClient = hackerNewsClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
    {
        var stories = await GetAllBestStoriesAsync(cancellationToken);
        return stories.Take(count).ToList();
    }

    private async Task<IReadOnlyList<StoryDto>> GetAllBestStoriesAsync(CancellationToken cancellationToken)
    {
        if(_memoryCache.TryGetValue(CacheKey, out IReadOnlyList<StoryDto>? stories) && stories is not null)
        {
            return stories;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if(_memoryCache.TryGetValue(CacheKey, out stories) && stories is not null)
            {
                return stories;
            }

            var fetchedStories = await FetchBestStoriesAsync(cancellationToken);
            _memoryCache.Set(CacheKey, fetchedStories, CacheDuration);
            return fetchedStories;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<IReadOnlyList<StoryDto>> FetchBestStoriesAsync(CancellationToken cancellationToken)
    {
        var storyIds = await _hackerNewsClient.GetBestStoryIdsAsync(cancellationToken);
        var  items = new ConcurrentBag<HackerNewsItem>();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxConcurrentRequests,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(storyIds, options, async (storyId, token) =>
        {
            try
            {
            var story = await _hackerNewsClient.GetStoryAsync(storyId, token);
            if(story is not null)
            {
                items.Add(story);
            }
            }
            catch(HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching story {StoryId}", storyId);
            }
            
        });

        return items.OrderByDescending(item => item.Score)
                     .Select(StoryDto.FromHackerNewsItem)
                     .ToList();        
}
}