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
.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IBestStoriesService, BestStoriesService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
