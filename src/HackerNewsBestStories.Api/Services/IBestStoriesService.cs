using HackerNewsBestStories.Api.Models;

namespace HackerNewsBestStories.Api.Services;

public interface IBestStoriesService
{
    Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
}