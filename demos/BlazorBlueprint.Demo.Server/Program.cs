using BlazorBlueprint.Demo.Extensions;
using BlazorBlueprint.Demo.Server;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add all demo services via shared extension method
builder.Services.AddBlazorBlueprintDemo();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapGet("/sse", async (HttpContext context, [FromHeader(Name = "Last-Event-ID")] string? lastEventId, CancellationToken cancellationToken) =>
{
    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";

    static async IAsyncEnumerable<SseItem<string>> GenerateEvents(string? lastEventId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        //Servers should respect the Last-Event-ID header and only send events that the client hasn't received yet.
        var start = 0;
        if (lastEventId != null && int.TryParse(lastEventId, out var lastId))
        {
            start = lastId + 1;
        }

        //For this example, send only the same 10 events to show that the component handles duplicate events on reconnects.
        foreach (var index in Enumerable.Range(0, 10))
        {
            yield return new SseItem<string>($"{{\"timestamp\":\"{DateTime.UtcNow:O}\"}}", "tick") { EventId = index.ToString(CultureInfo.InvariantCulture) };
            yield return new SseItem<string>($"{{\"text\":\"Message #{index + 1} at {DateTime.UtcNow:O}\"}}", "my-message") { EventId = index.ToString(CultureInfo.InvariantCulture) };
            await Task.Delay(2000, cancellationToken);
        }
    }

    //In .NET 10, all of the following can be simplified to just
    //return TypedResults.ServerSentEvents(IAsyncEnumerable<SseItem<TValue>>);
    //This will also set the correct content type and headers.
    await foreach (var item in GenerateEvents(lastEventId, cancellationToken))
    {
        await context.Response.WriteAsync($"event: {item.EventType}\n", cancellationToken);
        await context.Response.WriteAsync($"id: {item.EventId}\n", cancellationToken);
        await context.Response.WriteAsync($"data: {item.Data}", cancellationToken);
        await context.Response.WriteAsync($"\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }

    await context.Response.WriteAsync($"data: ", cancellationToken);
    await JsonSerializer.SerializeAsync(context.Response.Body, "event: tick\ndata: {\"timestamp\":\"done\"}\n\n", cancellationToken: cancellationToken);
    await context.Response.Body.FlushAsync(cancellationToken);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorBlueprint.Demo.Routes).Assembly);

app.Run();
