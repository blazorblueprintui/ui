using System.Globalization;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Presets that resolve to the same dates must not all highlight at once, and the desktop sidebar
/// and the mobile select must agree on the one that is (#582).
/// </summary>
public class DateRangePickerPresetTests
{
    private static readonly DateTime September1 = new(2026, 9, 1);
    private static readonly DateTime September30 = new(2026, 9, 30);
    private static readonly DateTime October1 = new(2026, 10, 1);

    // Default presets: Today, Yesterday, Last 7 days, Last 30 days, This month, Last month.
    private const int Last30Days = 3;
    private const int ThisMonth = 4;

    [Theory]
    [InlineData(Last30Days, false)]
    [InlineData(ThisMonth, false)]
    [InlineData(Last30Days, true)]
    [InlineData(ThisMonth, true)]
    public async Task PickedBuiltInPresetIsTheOnlyOneHighlightedWhenItsRangeCollides(int picked, bool fromSelect)
    {
        // On 30 September, Last 30 days and This month are both 1–30 September.
        await using var picker = await PickerHarness.OpenAsync(September30);

        await picker.Pick(picked, fromSelect);

        await picker.AssertHighlighted(picked);
    }

    [Fact]
    public async Task SwitchingBetweenCollidingPresetsMovesTheHighlight()
    {
        await using var picker = await PickerHarness.OpenAsync(September30);

        await picker.Pick(ThisMonth);
        await picker.AssertHighlighted(ThisMonth);

        await picker.Pick(Last30Days);
        await picker.AssertHighlighted(Last30Days);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task PickedPresetWinsOnTheFirstOfTheMonth(int picked)
    {
        // On 1 October, Today and a month-to-date preset are both 1–1 October.
        await using var picker = await PickerHarness.OpenAsync(October1, presets:
        [
            DateRangePreset.Today,
            DateRangeQuickPick.Custom("Month to date", () => new DateRange(October1, October1))
        ]);

        await picker.Pick(picked);

        await picker.AssertHighlighted(picked);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task CustomPresetEqualToABuiltInOneHighlightsOnlyThePickedOne(int picked)
    {
        await using var picker = await PickerHarness.OpenAsync(September30, presets:
        [
            DateRangeQuickPick.Custom("Past week", () => new DateRange(September30.AddDays(-6), September30)),
            DateRangePreset.Last7Days
        ]);

        await picker.Pick(picked);

        await picker.AssertHighlighted(picked);
    }

    [Fact]
    public async Task ChangingTheDatesByHandForgetsThePickedPreset()
    {
        await using var picker = await PickerHarness.OpenAsync(September30);
        await picker.Pick(ThisMonth);

        await picker.ClickDay(1);
        await picker.AssertHighlighted(null);

        // The hand-picked range equals both presets again; with nothing remembered the first match wins.
        await picker.ClickDay(30);
        await picker.AssertHighlighted(Last30Days);
    }

    [Fact]
    public async Task ValueFromOutsideHighlightsTheFirstMatchingPreset()
    {
        await using var picker = await PickerHarness.OpenAsync(September30, value: new DateRange(September1, September30));

        await picker.AssertHighlighted(Last30Days);
    }

    [Fact]
    public async Task BoundValueEchoingThePickedPresetKeepsIt()
    {
        DateRange? bound = null;
        await using var picker = await PickerHarness.OpenAsync(September30, autoApply: true, onValueChanged: value => bound = value);

        await picker.Pick(ThisMonth);
        Assert.Equal(new DateRange(September1, September30), bound);
        picker.SetValue(bound);
        await picker.Open();

        await picker.AssertHighlighted(ThisMonth);
    }

    [Fact]
    public async Task ValueFromOutsideThatDoesNotMatchForgetsThePickedPreset()
    {
        await using var picker = await PickerHarness.OpenAsync(September30);
        await picker.Pick(ThisMonth);

        picker.SetValue(new DateRange(September30, September30));
        picker.SetValue(new DateRange(September1, September30));

        await picker.AssertHighlighted(Last30Days);
    }

    private sealed class PickerHarness : IAsyncDisposable
    {
        private readonly ComponentBunitContext context = new();
        private readonly IRenderedComponent<BbPortalHost> portal;
        private readonly IRenderedComponent<BbDateRangePicker> picker;

        private PickerHarness(
            DateTime today,
            IReadOnlyList<DateRangeQuickPick>? presets,
            DateRange? value,
            bool autoApply,
            Action<DateRange?>? onValueChanged)
        {
            context.JSInterop.Mode = JSRuntimeMode.Loose;
            portal = context.Render<BbPortalHost>();
            picker = context.Render<BbDateRangePicker>(parameters => parameters
                .Add(p => p.Presets, presets)
                .Add(p => p.Value, value)
                .Add(p => p.AutoApply, autoApply)
                .Add(p => p.ValueChanged, onValueChanged ?? (_ => { })));
            picker.Instance.TodayProvider = () => today;
        }

        public static async Task<PickerHarness> OpenAsync(
            DateTime today,
            IReadOnlyList<DateRangeQuickPick>? presets = null,
            DateRange? value = null,
            bool autoApply = false,
            Action<DateRange?>? onValueChanged = null)
        {
            var harness = new PickerHarness(today, presets, value, autoApply, onValueChanged);
            await harness.Open();
            return harness;
        }

        public Task Open() => InPortal(content =>
        {
            if (content.FindAll("[data-drp-preset-btn]").Count == 0)
            {
                picker.Find("button").Click();
            }
        });

        public void SetValue(DateRange? value) =>
            picker.Render(parameters => parameters.Add(p => p.Value, value));

        public Task Pick(int index, bool fromSelect = false) => InPortal(content =>
        {
            if (fromSelect)
            {
                content.Find("select").Change(index.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                content.FindAll("[data-drp-preset-btn]")[index].Click();
            }
        });

        public Task ClickDay(int day) => InPortal(content =>
            content.FindAll("[data-drp-calendar] [data-drp-day-btn]")
                .First(button => button.TextContent.Trim() == day.ToString(CultureInfo.InvariantCulture))
                .Click());

        public Task AssertHighlighted(int? expected) => InPortal(content =>
        {
            var highlighted = content.FindAll("[data-drp-preset-btn]")
                .Select((button, index) => (button, index))
                .Where(pair => pair.button.ClassList.Contains("bb:bg-primary"))
                .Select(pair => pair.index)
                .ToArray();
            Assert.Equal(expected is int index ? [index] : [], highlighted);

            var selected = content.Find("select").GetAttribute("value");
            Assert.Equal(expected?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, selected ?? string.Empty);
        });

        // The portal host re-renders its content on a deferred dispatcher turn that bUnit doesn't
        // pick up, which can leave its markup holding retired event handlers. Re-render the host
        // first, in the same dispatcher turn as the action, so the test reads the current tree.
        private Task InPortal(Action<IRenderedComponent<BbPortalHost>> action) => portal.InvokeAsync(() =>
        {
            portal.Render();
            action(portal);
        });

        // Some of the services only support async disposal.
        public ValueTask DisposeAsync() => context.DisposeAsync();
    }
}
