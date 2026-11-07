using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Floating;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Selection;

/// <summary>
/// Escape in an open BbMultiSelect must close the list and nothing underneath it. Only the search
/// input caught Escape itself; once an option was clicked focus left the input, the list was not
/// on the shared Escape stack, and the key reached the dialog around it and closed that too.
/// </summary>
public class MultiSelectEscapeTests
{
    [Fact]
    public async Task OpenListTakesEscapeWhereverFocusIs()
    {
        await RunAsync(async (renderer, multiSelect) =>
        {
            await OpenAsync(multiSelect);

            var portal = renderer.FindComponent<BbFloatingPortal>();
            Assert.True(portal.Dismiss?.OnEscapeKey, "The open list must join the Escape stack so it sits above a dialog.");

            await portal.JsOnDismissEscape();

            Assert.False(IsOpen(multiSelect));
        });
    }

    [Fact]
    public async Task EscapeFromTheSearchInputStillClosesTheList()
    {
        await RunAsync(async (_, multiSelect) =>
        {
            await OpenAsync(multiSelect);

            await multiSelect.HandleEscape();

            Assert.False(IsOpen(multiSelect));
        });
    }

    private static async Task OpenAsync(BbMultiSelect<string> multiSelect)
    {
        ComponentProbe.Call(multiSelect, "Open");
        ComponentProbe.Call(multiSelect, "StateHasChanged");
        await Task.Yield();
        Assert.True(IsOpen(multiSelect));
    }

    private static bool IsOpen(BbMultiSelect<string> multiSelect) =>
        (bool)typeof(BbMultiSelect<string>)
            .GetProperty("_isOpen", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(multiSelect)!;

    private static async Task RunAsync(Func<ComponentTestRenderer, BbMultiSelect<string>, Task> test)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbPortalHost>(new());
            var multiSelect = await renderer.MountAsync<BbMultiSelect<string>>(new()
            {
                [nameof(BbMultiSelect<string>.Options)] = new[]
                {
                    new SelectOption<string>("a", "Alpha"),
                    new SelectOption<string>("b", "Beta")
                }
            });
            await test(renderer, multiSelect);
        });
    }
}
