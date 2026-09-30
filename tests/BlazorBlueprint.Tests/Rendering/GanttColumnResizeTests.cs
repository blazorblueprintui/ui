using System.Text.RegularExpressions;
using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Rendering;

/// <summary>
/// What a column resize drag is allowed to record in <see cref="BbGantt{TItem}"/>.
/// </summary>
/// <remarks>
/// The Gantt shares the DataGrid's column-resize script, so it can be told a column measured zero:
/// a column with nothing to measure reports no width. Recording that would draw the column at zero
/// and it could never be dragged back.
/// </remarks>
public class GanttColumnResizeTests
{
    private sealed record Job(string Id, string Name, DateTimeOffset Start, DateTimeOffset End);

    [Fact]
    public async Task ADragThatMeasuresAColumnAtZeroLeavesItsWidthAlone()
    {
        await WithGanttAsync(async (gantt, renderer) =>
        {
            await renderer.Dispatcher.InvokeAsync(() =>
                gantt.OnResizeCompleted("b", new Dictionary<string, double> { ["a"] = 0, ["b"] = 120.4 }));
            var markup = await SettleAsync(renderer);

            Assert.Equal("width:200px", ColStyle(markup, "a"));
            Assert.Equal("width:120px", ColStyle(markup, "b"));
        });
    }

    [Fact]
    public async Task ADragThatMeasuresNothingUsableRecordsNothing()
    {
        await WithGanttAsync(async (gantt, renderer) =>
        {
            await renderer.Dispatcher.InvokeAsync(() =>
                gantt.OnResizeCompleted("b", new Dictionary<string, double>
                {
                    ["a"] = double.NaN,
                    ["b"] = -40
                }));
            var markup = await SettleAsync(renderer);

            Assert.Equal("width:200px", ColStyle(markup, "a"));
            Assert.Equal("width:100px", ColStyle(markup, "b"));
        });
    }

    private static string? ColStyle(string markup, string columnId)
    {
        var match = Regex.Match(
            markup,
            $"<col data-column-id=\"{Regex.Escape(columnId)}\" style=\"([^\"]*)\"");

        return match.Success ? match.Groups[1].Value : null;
    }

    private static async Task WithGanttAsync(Func<BbGantt<Job>, ComponentTestRenderer, Task> test)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();

        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var jobs = new List<Job> { new("job-1", "Write the thing", start, start.AddDays(4)) };

        var parameters = new Dictionary<string, object?>
        {
            [nameof(BbGantt<Job>.Data)] = jobs,
            [nameof(BbGantt<Job>.IdSelector)] = (Func<Job, string>)(j => j.Id),
            [nameof(BbGantt<Job>.TextSelector)] = (Func<Job, string>)(j => j.Name),
            [nameof(BbGantt<Job>.StartSelector)] = (Func<Job, DateTimeOffset>)(j => j.Start),
            [nameof(BbGantt<Job>.EndSelector)] = (Func<Job, DateTimeOffset>)(j => j.End),
            [nameof(BbGantt<Job>.ScrollToToday)] = false,
            [nameof(BbGantt<Job>.Columns)] = (RenderFragment)(builder =>
            {
                Column(builder, 0, "a", 200);
                Column(builder, 10, "b", 100);
            }),
        };

        BbGantt<Job> gantt = default!;
        await renderer.Dispatcher.InvokeAsync(async () =>
            gantt = await renderer.MountAsync<BbGantt<Job>>(parameters));

        var markup = await SettleAsync(renderer);

        // Drawn at their declared widths first, or the assertions after the drag prove nothing.
        Assert.Equal("width:200px", ColStyle(markup, "a"));
        Assert.Equal("width:100px", ColStyle(markup, "b"));

        await test(gantt, renderer);
    }

    private static void Column(RenderTreeBuilder builder, int sequence, string key, int width)
    {
        builder.OpenComponent<BbGanttColumn<Job>>(sequence);
        builder.AddAttribute(sequence + 1, nameof(BbGanttColumn<Job>.Key), key);
        builder.AddAttribute(sequence + 2, nameof(BbGanttColumn<Job>.Title), key.ToUpperInvariant());
        builder.AddAttribute(sequence + 3, nameof(BbGanttColumn<Job>.Width), width);
        builder.CloseComponent();
    }

    // The chart is built in OnAfterRender, and a recorded width redraws it the same way.
    private static async Task<string> SettleAsync(ComponentTestRenderer renderer)
    {
        for (var i = 0; i < 6; i++)
        {
            await renderer.Dispatcher.InvokeAsync(() => { });
            await Task.Delay(10);
        }

        var markup = string.Empty;
        await renderer.Dispatcher.InvokeAsync(() => markup = renderer.Markup());
        return markup;
    }
}
