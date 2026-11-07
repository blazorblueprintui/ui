using BlazorBlueprint.Components;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Input-style fields and the outline button use shadcn's fill: see-through or the page colour in
/// light mode, a tint of the theme's <c>--input</c> in dark mode (#610).
/// </summary>
public class OutlineFillTests
{
    [Fact]
    public void OutlineButtonUsesTheShadcnFill()
    {
        using var context = new ComponentBunitContext();

        var button = context.Render<BbButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Outline)
            .AddChildContent("Outline"));

        var classes = button.Find("button").ClassList;
        Assert.Contains("bb:bg-background/100", classes);
        Assert.Contains("bb:dark:bg-input/30", classes);
        Assert.Contains("bb:dark:hover:bg-input/50", classes);
    }

    /// <summary>
    /// A background set through <c>Class</c> must replace the light fill. With a variant on the
    /// light fill (<c>not-dark:</c>) the merge kept both, and the light fill won.
    /// </summary>
    [Fact]
    public void OutlineButtonClassBackgroundReplacesTheLightFill()
    {
        using var context = new ComponentBunitContext();

        var button = context.Render<BbButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Outline)
            .Add(p => p.Class, "bb:bg-primary")
            .AddChildContent("Custom primary"));

        var classes = button.Find("button").ClassList;
        Assert.Contains("bb:bg-primary", classes);
        Assert.DoesNotContain(classes, c => c.Contains("bg-background", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EditableDatePickerTintsOnHoverInDarkMode()
    {
        await using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var picker = context.Render<BbDatePicker>(parameters => parameters
            .Add(p => p.Editable, true)
            .AddUnmatched("data-testid", "picker"));

        var classes = picker.Find("[data-testid='picker']").ClassList;
        Assert.Contains("bb:dark:bg-input/30", classes);
        Assert.Contains("bb:dark:hover:bg-input/50", classes);
    }
}
