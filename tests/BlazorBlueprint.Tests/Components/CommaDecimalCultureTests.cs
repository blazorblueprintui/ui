using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Icons.Feather.Components;
using BlazorBlueprint.Icons.Heroicons.Components;
using BlazorBlueprint.Icons.Lucide.Components;
using Bunit;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Renders components under cultures that write decimals with a comma, and checks that no number
/// reaches an inline style, an SVG attribute, an aria-value* attribute or a data-* attribute as
/// <c>1,75</c>.
/// <para>
/// The browser drops such a declaration in silence: under cs-CZ the tree lost its indentation
/// (#577). <see cref="Conventions.CultureInvariantStyleTests"/> scans Razor markup for the same
/// mistake, but cannot see a string built in a code-behind or a number bound straight to an SVG
/// attribute. Rendering catches both.
/// </para>
/// <para>
/// Screen readers parse aria-valuenow, -min and -max, and scripts parse data-* attributes, as
/// invariant numbers, so <c>33,3</c> there is not a number either.
/// </para>
/// </summary>
public class CommaDecimalCultureTests
{
    private static readonly Regex CommaDecimal = new(@"\d,\d", RegexOptions.Compiled);

    // Their grammar separates numbers with commas, so "10,20" there is valid SVG, not a decimal.
    private static readonly HashSet<string> ListValuedSvgAttributes =
        new(StringComparer.OrdinalIgnoreCase) { "d", "points", "viewBox", "transform", "stroke-dasharray" };

    // aria-valuetext is left out on purpose: it is read aloud as written, so it may follow the culture.
    private static readonly HashSet<string> AriaNumberAttributes =
        new(StringComparer.Ordinal) { "aria-valuenow", "aria-valuemin", "aria-valuemax" };

    public static TheoryData<string> CommaDecimalCultures => new() { "cs-CZ", "de-DE" };

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void DeclarativeTreeIndentsWithInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<BbTreeView<string>>(parameters => parameters
            .Add(p => p.DefaultExpandAll, true)
            .AddChildContent<BbTreeItem>(root => root
                .Add(p => p.Value, "src")
                .Add(p => p.Label, "src")
                .AddChildContent<BbTreeItem>(child => child
                    .Add(p => p.Value, "app")
                    .Add(p => p.Label, "app"))));

        Assert.Contains(cut.FindAll("[style]"), e => e.GetAttribute("style")!.Contains("padding-left: 1.75rem", StringComparison.Ordinal));
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void DataDrivenTreeIndentsWithInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var items = new[] { new Node("src", [new Node("app", [])]) };

        var cut = context.Render<BbTreeView<Node>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ChildrenProperty, n => n.Children)
            .Add(p => p.ValueField, n => n.Name)
            .Add(p => p.TextField, n => n.Name)
            .Add(p => p.DefaultExpandAll, true));

        Assert.Contains(cut.FindAll("[style]"), e => e.GetAttribute("style")!.Contains("padding-left: 1.75rem", StringComparison.Ordinal));
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void HalfRatingWritesInvariantGradients(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();

        foreach (var icon in Enum.GetValues<RatingIcon>())
        {
            var cut = context.Render<BbRating>(parameters => parameters
                .Add(p => p.Value, 2.5)
                .Add(p => p.AllowHalf, true)
                .Add(p => p.Icon, icon));

            Assert.Contains(cut.FindAll("stop"), e => e.GetAttribute("offset") == "50%");
            AssertNoCommaDecimals(cut.FindAll("*"));
        }
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void ProgressTranslatesWithInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();

        var cut = context.Render<BbProgress>(parameters => parameters.Add(p => p.Value, 33.3));

        Assert.Contains(cut.FindAll("[style]"), e => e.GetAttribute("style") == "transform: translateX(-66.7%)");
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void ProgressAnnouncesInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();

        var cut = context.Render<BbProgress>(parameters => parameters
            .Add(p => p.Value, 33.3)
            .Add(p => p.Max, 99.9));

        var bar = cut.Find("[role='progressbar']");
        Assert.Equal("33.3", bar.GetAttribute("aria-valuenow"));
        Assert.Equal("99.9", bar.GetAttribute("aria-valuemax"));
        Assert.Equal(2, cut.FindAll("[data-value='33.3'][data-max='99.9']").Count);
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void RangeSliderAnnouncesInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<BbRangeSlider>(parameters => parameters
            .Add(p => p.Min, 0.5)
            .Add(p => p.Max, 99.5)
            .Add(p => p.Step, 0.5)
            .Add(p => p.DefaultStart, 12.5)
            .Add(p => p.DefaultEnd, 87.5));

        var thumbs = cut.FindAll("[role='slider']");
        Assert.Equal(["12.5", "87.5"], thumbs.Select(t => t.GetAttribute("aria-valuenow")));
        Assert.All(thumbs, t => Assert.Equal("0.5", t.GetAttribute("aria-valuemin")));
        Assert.All(thumbs, t => Assert.Equal("99.5", t.GetAttribute("aria-valuemax")));
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void NumericInputAnnouncesInvariantNumbers(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<BbNumericInput<double>>(parameters => parameters
            .Add(p => p.Value, 2.5)
            .Add(p => p.Min, 0.5)
            .Add(p => p.Max, 9.5));

        var input = cut.Find("input");
        Assert.Equal("2.5", input.GetAttribute("aria-valuenow"));
        Assert.Equal("0.5", input.GetAttribute("aria-valuemin"));
        Assert.Equal("9.5", input.GetAttribute("aria-valuemax"));
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void ContextMenuOpensAtInvariantCoordinates(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<BbContextMenu>(parameters => parameters
            .AddChildContent<BbContextMenuTrigger>(trigger => trigger
                .AddUnmatched("data-testid", "area")
                .AddChildContent("Right-click here"))
            .AddChildContent<BbContextMenuContent>(content => content
                .AddChildContent<BbContextMenuItem>(item => item.AddChildContent("Copy"))));

        cut.Find("[data-testid='area']").ContextMenu(new MouseEventArgs { ClientX = 120.5, ClientY = 64.25 });

        Assert.Contains("left: 120.5px; top: 64.25px", cut.Find("[role='menu']").GetAttribute("style"), StringComparison.Ordinal);
        AssertNoCommaDecimals(cut.FindAll("*"));
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public void IconsWriteInvariantStrokeWidths(string culture) => InCulture(culture, () =>
    {
        using var context = new ComponentBunitContext();

        var lucide = context.Render<LucideIcon>(parameters => parameters
            .Add(p => p.Name, "a-arrow-down")
            .Add(p => p.StrokeWidth, 1.5));
        var feather = context.Render<FeatherIcon>(parameters => parameters
            .Add(p => p.Name, "activity")
            .Add(p => p.StrokeWidth, 1.5));
        // Outline Heroicons default to 1.5, so every one of them was affected.
        var hero = context.Render<HeroIcon>(parameters => parameters.Add(p => p.Name, "academic-cap"));

        foreach (var svg in new[] { lucide.Find("svg"), feather.Find("svg"), hero.Find("svg") })
        {
            Assert.Equal("1.5", svg.GetAttribute("stroke-width"));
            AssertNoCommaDecimals(svg.QuerySelectorAll("*").Prepend(svg));
        }
    });

    [Theory]
    [MemberData(nameof(CommaDecimalCultures))]
    public Task NumericInputKeepsDecimalsThroughFocusAndBlur(string culture) => InCulture(culture, async () =>
    {
        using var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var value = 33.3;

        var cut = context.Render<BbNumericInput<double>>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, v => value = v));
        var rendered = cut.Find("input").GetAttribute("value");

        await cut.InvokeAsync(cut.Instance.JsOnFocus);
        var editing = cut.Find("input").GetAttribute("value");

        // The parser treats a comma as a thousands separator, so blurring on "33,3" committed 333.
        await cut.InvokeAsync(() => cut.Instance.JsOnBlur(editing));

        Assert.Multiple(
            () => Assert.Equal("33.3", rendered),
            () => Assert.Equal("33.3", editing),
            () => Assert.Equal(33.3, value));
    });

    private static void InCulture(string name, Action test) =>
        InCulture(name, () =>
        {
            test();
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();

    private static async Task InCulture(string name, Func<Task> test)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

            // Under invariant globalization every culture formats like the invariant one, and these
            // tests would pass without proving anything.
            Assert.Equal(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            await test();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private static void AssertNoCommaDecimals(IEnumerable<IElement> elements)
    {
        var offenders = new List<string>();

        foreach (var element in elements)
        {
            var inSvg = element.Closest("svg") is not null;

            foreach (var attribute in element.Attributes)
            {
                var isCss = attribute.Name == "style";
                var isSvgNumber = inSvg && !ListValuedSvgAttributes.Contains(attribute.Name);
                var isParsedNumber = AriaNumberAttributes.Contains(attribute.Name)
                    || attribute.Name.StartsWith("data-", StringComparison.Ordinal);

                if ((isCss || isSvgNumber || isParsedNumber) && CommaDecimal.IsMatch(attribute.Value))
                {
                    offenders.Add($"<{element.LocalName} {attribute.Name}=\"{attribute.Value}\">");
                }
            }
        }

        Assert.Empty(offenders);
    }

    public sealed record Node(string Name, IReadOnlyList<Node> Children);
}
