using System.Text.RegularExpressions;
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
    /// Every BbFormField* control points <c>aria-describedby</c> at the helper text, and
    /// BbFieldDescription took an <c>Id</c> and never rendered it — so the reference dangled and a
    /// screen reader read out no description at all.
    /// </summary>
    [Fact]
    public async Task HelperTextIdIsActuallyRendered()
    {
        var markup = await Render<BbFormFieldCheckbox>(new()
        {
            [nameof(BbFormFieldCheckbox.HelperText)] = "Pick one"
        });

        var described = Regex.Match(markup, @"aria-describedby=""(?<id>[^""]+)""");
        Assert.True(described.Success, $"No aria-describedby was rendered:\n{markup}");
        Assert.Contains($"id=\"{described.Groups["id"].Value}\"", markup, StringComparison.Ordinal);
    }

    /// <summary>The same for the error text, which uses its own id.</summary>
    [Fact]
    public async Task ErrorTextIdIsActuallyRendered()
    {
        var markup = await Render<BbFormFieldCheckbox>(new()
        {
            [nameof(BbFormFieldCheckbox.ErrorText)] = "Required"
        });

        var described = Regex.Match(markup, @"aria-describedby=""(?<id>[^""]+)""");
        Assert.True(described.Success, $"No aria-describedby was rendered:\n{markup}");
        Assert.Contains($"id=\"{described.Groups["id"].Value}\"", markup, StringComparison.Ordinal);
    }

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

    /// <summary>
    /// Every toast had <c>role="alert"</c>, so a screen reader interrupted the user for "Saved" as
    /// it would for an error. All stacks now sit in one polite live region that exists before any
    /// toast arrives, and only a destructive toast is an alert.
    /// </summary>
    [Fact]
    public async Task OnlyDestructiveToastsAreAlerts()
    {
        await using var provider = Services().BuildServiceProvider();
        var toasts = provider.GetRequiredService<ToastService>();
        toasts.Info("Units: Imperial");
        toasts.Error("Save failed");
        toasts.Show(new ToastData { Description = "Elsewhere", Position = ToastPosition.TopLeft });

        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var markup = string.Empty;
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbToastProvider>([]);
            markup = renderer.Markup();
        });

        Assert.Matches(@"^<section[^>]* aria-label=""Notifications"" aria-live=""polite"">", markup);
        Assert.EndsWith("</section>", markup, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(markup, "role=\"alert\""));
        var alert = markup.IndexOf("role=\"alert\"", StringComparison.Ordinal);
        Assert.InRange(alert, markup.IndexOf("Units: Imperial", StringComparison.Ordinal), markup.IndexOf("Save failed", StringComparison.Ordinal));
    }

    /// <summary>
    /// A live region only announces content added after it exists, so the region must be in the
    /// markup while there are no toasts at all.
    /// </summary>
    [Fact]
    public async Task ToastRegionExistsBeforeAnyToast()
    {
        var markup = await Render<BbToastProvider>([]);

        Assert.Matches(@"^<section[^>]* aria-label=""Notifications"" aria-live=""polite""><div[^>]*></div></section>$", markup);
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
