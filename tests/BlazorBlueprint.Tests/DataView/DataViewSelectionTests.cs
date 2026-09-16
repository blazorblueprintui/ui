using BlazorBlueprint.Components;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.DataView;

public class DataViewSelectionTests
{
    [Fact]
    public async Task StableKeysRetainSelectionAfterReloadAndSingleSelectionReplacesThePreviousItem()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var original = new Item(1, "Before", "A");
            var view = await renderer.MountAsync<BbDataView<Item>>(new()
            {
                [nameof(BbDataView<Item>.Data)] = new[] { original, new Item(2, "Second", "A") },
                [nameof(BbDataView<Item>.ShowToolbar)] = false,
                [nameof(BbDataView<Item>.ShowPagination)] = false,
                [nameof(BbDataView<Item>.ItemKey)] = (Func<Item, object>)(item => item.Id),
                [nameof(BbDataView<Item>.IsItemDisabled)] = (Func<Item, bool>)(item => item.Id == 3),
                [nameof(BbDataView<Item>.SelectionMode)] = DataTableSelectionMode.Multiple
            });
            await (Task)ComponentProbe.Call(view, "ToggleSelectionAsync", original, true)!;
            var reloaded = original with { Name = "After" };
            await view.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(view.Data)] = new[] { reloaded } }));
            Assert.True((bool)ComponentProbe.Call(view, "IsSelected", reloaded)!);
            await (Task)ComponentProbe.Call(view, "ToggleSelectionAsync", reloaded, false)!;
            Assert.Empty(view.SelectedItems);
            await view.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(view.SelectionMode)] = DataTableSelectionMode.Single }));
            await (Task)ComponentProbe.Call(view, "ToggleSelectionAsync", original, true)!;
            var second = new Item(2, "Second", "B");
            await (Task)ComponentProbe.Call(view, "ToggleSelectionAsync", second, true)!;
            await (Task)ComponentProbe.Call(view, "ToggleSelectionAsync", new Item(3, "Disabled", "B"), true)!;
            Assert.Equal([second], view.SelectedItems);
        });
    }

    [Fact]
    public async Task GroupsReflectLoadedItemsAndHidingPaginationExposesAllLocalData()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var items = Enumerable.Range(0, 30).Select(i => new Item(i, $"Item {i}", i < 15 ? "A" : "B")).ToList();
            RenderFragment<Item> template = item => builder => builder.AddContent(0, item.Name);
            var view = await renderer.MountAsync<BbDataView<Item>>(new()
            {
                [nameof(BbDataView<Item>.Data)] = items,
                [nameof(BbDataView<Item>.ShowToolbar)] = false,
                [nameof(BbDataView<Item>.ShowPagination)] = false,
                [nameof(BbDataView<Item>.GroupBy)] = (Func<Item, string?>)(item => item.Group),
                [nameof(BbDataView<Item>.ListTemplate)] = template,
                [nameof(BbDataView<Item>.GridTemplate)] = template
            });
            var groups = ((IEnumerable<DataViewGroupContext<Item>>)ComponentProbe.Call(view, "get_VisibleGroups")!).ToList();
            Assert.Equal(2, groups.Count);
            Assert.All(groups, group => Assert.Equal(15, group.Items.Count));
            ComponentProbe.Call(view, "SetLayout", DataViewLayout.Grid);
            await view.SetParametersAsync(ParameterView.Empty);
            Assert.Equal(DataViewLayout.Grid, ComponentProbe.Field<DataViewLayout>(view, "currentLayout"));
            await view.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(view.ShowPagination)] = true }));
            Assert.Equal(5, ComponentProbe.Field<List<Item>>(view, "_visibleData").Count);
        });
    }

    [Fact]
    public async Task MobileToolbarOpenAndCloseReachTheSheetDespiteRenderCaching()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var view = await renderer.MountAsync<BbDataView<Item>>(new()
            {
                [nameof(BbDataView<Item>.MobileToolbar)] = true,
                [nameof(BbDataView<Item>.FilterContent)] = (RenderFragment)(builder => builder.AddContent(0, "Filters"))
            });
            Assert.False(renderer.FindComponent<BbSheet>().Open);
            ComponentProbe.Call(view, "set_mobileToolbarOpen", true);
            ComponentProbe.Call(view, "StateHasChanged");
            Assert.True(renderer.FindComponent<BbSheet>().Open);
            ComponentProbe.Call(view, "set_mobileToolbarOpen", false);
            ComponentProbe.Call(view, "StateHasChanged");
            Assert.False(renderer.FindComponent<BbSheet>().Open);
        });
    }

    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        return services;
    }

    private sealed record Item(int Id, string Name, string Group);
}
