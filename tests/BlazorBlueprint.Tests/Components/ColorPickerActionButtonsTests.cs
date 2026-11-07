using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// With <see cref="BbColorPicker.ActionButtons"/>, picking a color only stages it: Apply commits it
/// and Cancel discards it. Without it, a pick still commits straight away.
/// </summary>
public class ColorPickerActionButtonsTests
{
    private const string Initial = "#3B82F6";
    private const string Picked = "#EF4444";

    [Fact]
    public async Task ApplyCommitsTheStagedColor()
    {
        await using var picker = await PickerHarness.OpenAsync(actionButtons: true);

        await picker.ClickPreset(Picked);
        Assert.Empty(picker.Changes);

        await picker.ClickButton("Apply");
        Assert.Equal([Picked], picker.Changes);
    }

    [Fact]
    public async Task CancelDiscardsTheStagedColor()
    {
        await using var picker = await PickerHarness.OpenAsync(actionButtons: true);

        await picker.ClickPreset(Picked);
        await picker.ClickButton("Cancel");

        Assert.Empty(picker.Changes);
        Assert.Equal(Initial, picker.Value);
    }

    [Fact]
    public async Task WithoutActionButtonsAPickCommitsImmediately()
    {
        await using var picker = await PickerHarness.OpenAsync(actionButtons: false);

        await picker.ClickPreset(Picked);

        Assert.Equal([Picked], picker.Changes);
        await picker.AssertNoButton("Apply");
    }

    private sealed class PickerHarness : IAsyncDisposable
    {
        private readonly ComponentBunitContext context = new();
        private readonly IRenderedComponent<BbPortalHost> portal;
        private readonly IRenderedComponent<BbColorPicker> picker;

        private PickerHarness(bool actionButtons)
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            portal = context.Render<BbPortalHost>();
            picker = context.Render<BbColorPicker>(parameters => parameters
                .Add(p => p.Value, Initial)
                .Add(p => p.ActionButtons, actionButtons)
                .Add(p => p.PresetColors, [Initial, Picked])
                .Add(p => p.ValueChanged, value => Changes.Add(value)));
        }

        public List<string> Changes { get; } = [];

        public string Value => picker.Instance.Value;

        public static async Task<PickerHarness> OpenAsync(bool actionButtons)
        {
            var harness = new PickerHarness(actionButtons);
            await harness.picker.InvokeAsync(() => harness.picker.Find("button").Click());
            return harness;
        }

        public Task ClickPreset(string color) => InPortal(content =>
            content.Find($"button[title='{color}']").Click());

        public Task ClickButton(string text) => InPortal(content =>
            content.FindAll("button").Single(button => button.TextContent.Trim() == text).Click());

        public Task AssertNoButton(string text) => InPortal(content =>
            Assert.DoesNotContain(content.FindAll("button"), button => button.TextContent.Trim() == text));

        // Re-render the portal host in the same dispatcher turn as the action, so the test reads
        // the current tree (see DateRangePickerPresetTests).
        private Task InPortal(Action<IRenderedComponent<BbPortalHost>> action) => portal.InvokeAsync(() =>
        {
            portal.Render();
            action(portal);
        });

        public ValueTask DisposeAsync() => context.DisposeAsync();
    }
}
