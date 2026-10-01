using Microsoft.AspNetCore.Mvc;
using HackerNewsBestStories.Api.Services;
using HackerNewsBestStories.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace HackerNewsBestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BestStoriesController : ControllerBase
{
    private const int MaxStories = 200;
    private readonly IBestStoriesService _bestStoriesService;

    public BestStoriesController(IBestStoriesService bestStoriesService)
    {
        _bestStoriesService = bestStoriesService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<StoryDto>>> Get(
        [FromQuery, Range(1, MaxStories)] int n,
        CancellationToken cancellationToken)
    {
        var stories = await _bestStoriesService.GetBestStoriesAsync(n, cancellationToken);
        return Ok(stories);
    }
}