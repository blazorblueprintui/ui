using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.Select;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Selection;

public class SelectTriggerLifecycleTests
{
    [Fact]
    public async Task KeyboardActivationDoesNotCloseTheSelectItJustOpened()
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
            var select = await renderer.MountAsync<BlazorBlueprint.Primitives.Select.BbSelect<string>>(new()
            {
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<BlazorBlueprint.Primitives.Select.BbSelectTrigger<string>>(0);
                    builder.CloseComponent();
                })
            });
            var trigger = renderer.FindComponent<BlazorBlueprint.Primitives.Select.BbSelectTrigger<string>>();
            var context = ComponentProbe.Field<SelectContext<string>>(select, "_context");
            ComponentProbe.Call(trigger, "HandleKeyDown", new KeyboardEventArgs { Key = "Enter" });
            Assert.True(context.IsOpen);
            ComponentProbe.Call(trigger, "HandleClick", new MouseEventArgs { Detail = 0 });
            Assert.True(context.IsOpen);
            ComponentProbe.Call(trigger, "HandleClick", new MouseEventArgs { Detail = 1 });
            Assert.False(context.IsOpen);
        });
    }
}
