using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

using ToggleGroup = BlazorBlueprint.Primitives.Toggle.BbToggleGroup<string>;
using ToggleItem = BlazorBlueprint.Primitives.Toggle.BbToggleGroupItem<string>;
using PrimitiveToggleType = BlazorBlueprint.Primitives.Toggle.ToggleGroupType;

namespace BlazorBlueprint.Tests.Rendering;

/// <summary>
/// Accessibility defects that only show up in the rendered output: an <c>aria-describedby</c>
/// pointing at an id nothing writes, a state attribute with no value, a key press handled twice.
/// </summary>
public class AriaAndKeyboardTests
{
    /// <summary>
    /// The state attributes take the strings "true" and "false". Bound to a bool, Blazor renders
    /// the attribute with an empty value when true and drops it when false.
    /// </summary>
    [Fact]
    public async Task CollapsibleReportsItsExpandedStateAsAValue()
    {
        var closed = await Render<BlazorBlueprint.Primitives.Collapsible.BbCollapsible>(new()
        {
            [nameof(BlazorBlueprint.Primitives.Collapsible.BbCollapsible.ChildContent)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<BlazorBlueprint.Primitives.Collapsible.BbCollapsibleTrigger>(0);
                builder.CloseComponent();
                builder.OpenComponent<BlazorBlueprint.Primitives.Collapsible.BbCollapsibleContent>(1);
                builder.AddAttribute(2, "ForceMount", true);
                builder.CloseComponent();
            })
        });

        Assert.Contains("aria-expanded=\"false\"", closed, StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", closed, StringComparison.Ordinal);
    }

    /// <summary>
    /// A toggle group that allows one choice is a radio group, so its items carry
    /// <c>aria-checked</c>. They carried <c>aria-pressed</c>, which belongs to a button.
    /// </summary>
    [Fact]
    public async Task SingleSelectToggleGroupIsARadioGroup()
    {
        var markup = await Render<ToggleGroup>(new()
        {
            [nameof(ToggleGroup.Type)] = PrimitiveToggleType.Single,
            [nameof(ToggleGroup.DefaultValue)] = "a",
            [nameof(ToggleGroup.ChildContent)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<ToggleItem>(0);
                builder.AddAttribute(1, nameof(ToggleItem.Value), "a");
                builder.CloseComponent();
            })
        });

        Assert.Contains("role=\"radiogroup\"", markup, StringComparison.Ordinal);
        Assert.Contains("role=\"radio\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-checked=\"true\"", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-pressed", markup, StringComparison.Ordinal);
    }

    /// <summary>A multi-select group keeps the button role, and with it aria-pressed.</summary>
    [Fact]
    public async Task MultiSelectToggleGroupKeepsTheButtonRole()
    {
        var markup = await Render<ToggleGroup>(new()
        {
            [nameof(ToggleGroup.Type)] = PrimitiveToggleType.Multiple,
            [nameof(ToggleGroup.Values)] = new List<string> { "a" },
            [nameof(ToggleGroup.ValuesChanged)] = EventCallback.Factory.Create<List<string>>(this, _ => { }),
            [nameof(ToggleGroup.ChildContent)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<ToggleItem>(0);
                builder.AddAttribute(1, nameof(ToggleItem.Value), "a");
                builder.CloseComponent();
            })
        });

        Assert.Contains("role=\"group\"", markup, StringComparison.Ordinal);
        Assert.Contains("aria-pressed=\"true\"", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-checked", markup, StringComparison.Ordinal);
    }

    /// <summary>
    /// The radio group suppressed the default action of every key, so Tab could not move focus out
    /// of it. Only the arrow keys it acts on may be suppressed, and only while one is held.
    /// </summary>
    [Fact]
    public async Task RadioGroupDoesNotSwallowEveryKey()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbRadioGroup<string>>([]);
            Assert.DoesNotContain(PreventDefault, renderer.Markup(), StringComparison.Ordinal);

            // An arrow key with no items to move between must still not start suppressing Tab.
            await renderer.DispatchAsync("onkeydown", new KeyboardEventArgs { Key = "Tab" });
            Assert.DoesNotContain(PreventDefault, renderer.Markup(), StringComparison.Ordinal);
        });
    }

    /// <summary>
    /// The switch is a native button, so the browser raises a click of its own for Space. Handling
    /// the key as well toggled twice and the switch ended the press exactly where it started.
    /// </summary>
    [Fact]
    public async Task SpaceOnTheSwitchTogglesOnce()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var toggles = 0;
            var isChecked = false;

            await renderer.MountAsync<BlazorBlueprint.Primitives.Switch.BbSwitch>(new()
            {
                [nameof(BlazorBlueprint.Primitives.Switch.BbSwitch.Checked)] = isChecked,
                [nameof(BlazorBlueprint.Primitives.Switch.BbSwitch.CheckedChanged)] =
                    EventCallback.Factory.Create<bool>(this, value =>
                    {
                        isChecked = value;
                        toggles++;
                    })
            });

            // What a browser does for one Space press on a focused button: a keydown, then its own
            // click once the key is released.
            await DispatchIfPresent(renderer, "onkeydown", new KeyboardEventArgs { Key = " " });
            await renderer.DispatchAsync("onclick", new MouseEventArgs());

            Assert.Equal(1, toggles);
            Assert.True(isChecked);
        });
    }

    /// <summary>
    /// A hover card writes the side it opened on so the slide-in animation can match it. It wrote
    /// the enum name, "Bottom", and every selector looks for "bottom".
    /// </summary>
    [Fact]
    public async Task HoverCardWritesItsSideInLowerCase()
    {
        var markup = await Render<BbHoverCard>(new()
        {
            [nameof(BbHoverCard.DefaultOpen)] = true,
            [nameof(BbHoverCard.ChildContent)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<BbHoverCardContent>(0);
                builder.CloseComponent();
            })
        });

        Assert.DoesNotContain("data-side=\"Bottom\"", markup, StringComparison.Ordinal);
    }

    /// <summary>The attribute Blazor emits for a render-time <c>preventDefault</c> on key-down.</summary>
    private const string PreventDefault = "__internal_preventDefault_onkeydown";

    private static async Task DispatchIfPresent(ComponentTestRenderer renderer, string attribute, EventArgs args)
    {
        try
        {
            await renderer.DispatchAsync(attribute, args);
        }
        catch (InvalidOperationException)
        {
            // Nothing listens for the key any more, which is the point of the fix.
        }
    }

    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var markup = string.Empty;

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<T>(parameters);
            markup = renderer.Markup();
        });

        return markup;
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
