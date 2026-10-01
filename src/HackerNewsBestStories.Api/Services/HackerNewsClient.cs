using HackerNewsBestStories.Api.Models;

namespace HackerNewsBestStories.Api.Services;

public sealed class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

     public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken) =>
        await _httpClient.GetFromJsonAsync<int[]>("beststories.json", cancellationToken) ?? [];
    public  Task<HackerNewsItem?> GetStoryAsync(int storyId, CancellationToken cancellationToken) =>
        _httpClient.GetFromJsonAsync<HackerNewsItem>($"item/{storyId}.json", cancellationToken);

}

    
