# Hacker News Best Stories API

An ASP.NET Core Web API that returns the best `n` stories from the
[Hacker News API](https://github.com/HackerNews/API), sorted by score (highest first).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## How to run

```bash
dotnet run --project src/HackerNewsBestStories.Api
```

The API listens on `http://localhost:5137`.

```
GET http://localhost:5137/api/beststories?n=10
```

Example response:

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

| Request | Response |
|---|---|
| `?n=10` | `200 OK` with up to 10 stories |
| `?n=0`, `?n=201`, `?n=abc`, or `n` missing | `400 Bad Request` (ProblemDetails) |
| Hacker News unavailable | `500` (ProblemDetails) |

## Running the tests

```bash
dotnet test
```

## How it works

```
BestStoriesController  →  BestStoriesService  →  HackerNewsClient  →  Hacker News API
   (validation)          (cache, sort, take n)     (HTTP only)
```

To avoid overloading the Hacker News API while serving many requests:

- **Caching:** the full sorted list of best stories is cached in memory for 5 minutes.
  Any value of `n` is served from that one cache entry, so Hacker News is called
  at most about once every 5 minutes, however much traffic the API gets.
- **Request coalescing:** when the cache is empty, a `SemaphoreSlim` with
  double-checked locking ensures only one request fetches from Hacker News.
  Concurrent requests wait and then read the freshly cached result.
- **Bounded concurrency:** story details are fetched with `Parallel.ForEachAsync`,
  limited to 10 requests to Hacker News at a time.
- **Resilience:** `Microsoft.Extensions.Http.Resilience` adds retries with exponential
  backoff, per-attempt and total timeouts, and a circuit breaker.
- **Graceful degradation:** if a single story fails to load, it is logged and skipped
  instead of failing the whole response.
- **HttpClient lifetime:** the service is a singleton (so the cache lock is shared),
  so the typed `HttpClient` uses `SocketsHttpHandler.PooledConnectionLifetime` to
  pick up DNS changes.

## Assumptions

- `n` must be between 1 and 200, because `beststories.json` returns at most 200 IDs.
- Data up to 5 minutes old is acceptable. Scores change slowly, and freshness is
  traded for protecting the Hacker News API.
- Stories that are deleted or fail to load are left out, so a response can contain
  fewer than `n` stories.
- `uri` is `null` for posts without a link (for example, "Ask HN" posts).
- Stories are ordered by their current `score`, not by Hacker News's own "best" ranking.
- The API runs as a single instance, so an in-memory cache is enough.

## Enhancements given more time
- **Background refresh:** refresh the cache on a timer with a hosted service, so no
  user request ever waits for Hacker News, and serve the last good data if Hacker News is down.
- **Distributed cache (for example, Redis):** share the cache across multiple instances
  when scaling out.
- **Configuration:** move cache duration, concurrency limit, and base URL into
  `appsettings.json` using the Options pattern.
- **Incremental updates:** cache stories individually and fetch only new IDs on refresh.
- **Rate limiting** on this API itself, using ASP.NET Core's built-in rate limiter.
- **Integration tests** using `WebApplicationFactory` to test HTTP validation end to end.
- **Observability:** health checks, OpenTelemetry metrics and tracing.
- **Docker** support and a CI pipeline (GitHub Actions) that runs the tests.