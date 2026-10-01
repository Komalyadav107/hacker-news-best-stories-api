using HackerNewsBestStories.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client => 
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
})
.SetHandlerLifetime(Timeout.InfiniteTimeSpan)
.AddStandardResilienceHandler(options => {
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
    options.Retry.MaxRetryAttempts = 3;
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IBestStoriesService, BestStoriesService>();
builder.Services.AddProblemDetails(); 

builder.Services.AddOpenApi();


var app = builder.Build();

app.UseExceptionHandler();  

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
