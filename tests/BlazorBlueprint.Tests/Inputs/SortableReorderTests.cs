using BlazorBlueprint.Primitives.Sortable;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Inputs;

public class SortableReorderTests
{
    [Fact]
    public async Task RejectedAndStaleMovesDoNotReachTheConsumer()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var calls = new List<(int, int)>();
            var sortable = await renderer.MountAsync<BbSortable<string>>(new()
            {
                [nameof(BbSortable<string>.Items)] = new List<string> { "Pinned", "Move me", "Last" },
                [nameof(BbSortable<string>.CanMove)] = (Func<SortableMoveContext<string>, bool>)(move => move.OldIndex > 0 && move.NewIndex > 0),
                [nameof(BbSortable<string>.OnUpdate)] = EventCallback.Factory.Create<(int, int)>(calls, calls.Add)
            });
            await sortable.OnUpdateJS(0, 1);
            await sortable.OnUpdateJS(1, 0);
            await sortable.OnUpdateJS(3, 1);
            Assert.Empty(calls);
            await sortable.OnUpdateJS(1, 2);
            Assert.Equal([(1, 2)], calls);
            await sortable.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(BbSortable<string>.Sort)] = false }));
            await sortable.OnUpdateJS(1, 2);
            Assert.Single(calls);
            await sortable.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(sortable.CanDrop)] = (Func<SortableDropContext<string>, bool>)(drop => drop.Item != "Pinned" && drop.TargetId == "accepted")
            }));
            Assert.False(sortable.CanDropJS(0, 0, "accepted", false));
            Assert.False(sortable.CanDropJS(1, 0, "rejected", false));
            Assert.True(sortable.CanDropJS(1, 0, "accepted", false));
            Assert.Single(calls);
        });
    }
}
