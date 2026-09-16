using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives.Toggle;
using BlazorBlueprint.Tests.Performance;
using BlazorBlueprint.Tests.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using PrimitiveToggle = BlazorBlueprint.Primitives.Toggle.BbToggleGroup<string>;
using PrimitiveToggleType = BlazorBlueprint.Primitives.Toggle.ToggleGroupType;

namespace BlazorBlueprint.Tests.Navigation;

public class CarouselAndToggleTests
{
    [Fact]
    public async Task CarouselClampsToVisiblePagesLoopsAndReportsOnlyChanges()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var changes = new List<int>();
            var carousel = await renderer.MountAsync<BbCarousel>(new()
            {
                [nameof(BbCarousel.SlidesPerView)] = 2.0,
                [nameof(BbCarousel.OnSlideChanged)] = EventCallback.Factory.Create<int>(this, value => changes.Add(value))
            });
            for (var i = 0; i < 5; i++) { carousel.RegisterItem(); }
            await carousel.GoToAsync(99);
            Assert.Equal(3, carousel.ActiveIndex);
            Assert.False(carousel.CanGoNext);
            await carousel.GoToAsync(99);
            Assert.Single(changes);
            await carousel.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(carousel.Loop)] = true }));
            await carousel.Next();
            Assert.Equal(0, carousel.ActiveIndex);
            await carousel.Previous();
            Assert.Equal(3, carousel.ActiveIndex);
            await carousel.JsOnLayout(1);
            Assert.Equal(1, carousel.ActiveIndex);
            Assert.Equal([3, 0, 3, 1], changes);
        });
    }

    [Fact]
    public async Task RequiredToggleKeepsLastSelectionAndUncontrolledContextTracksChanges()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var toggle = await renderer.MountAsync<PrimitiveToggle>(new()
            {
                [nameof(PrimitiveToggle.Required)] = true,
                [nameof(PrimitiveToggle.DefaultValue)] = "one"
            });
            var context = ComponentProbe.Field<ToggleGroupContext<string>>(toggle, "context");
            await context.ToggleItem!("one");
            Assert.Equal("one", context.Value);
            await context.ToggleItem("two");
            Assert.Equal("two", context.Value);
            await toggle.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(toggle.Type)] = PrimitiveToggleType.Multiple }));
            await context.ToggleItem("one");
            await context.ToggleItem("one");
            Assert.Equal(["one"], context.Values);
            await context.ToggleItem("two");
            await context.ToggleItem("one");
            Assert.Equal(["two"], context.Values);
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
}
