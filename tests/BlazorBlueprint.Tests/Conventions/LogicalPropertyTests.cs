using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Spacing, borders, radii and text alignment are written with Tailwind's logical utilities
/// (<c>ms-</c>, <c>pe-</c>, <c>border-s</c>, <c>start-</c>, <c>text-start</c>) rather than their
/// physical counterparts, so the library mirrors under <c>dir="rtl"</c> without a second
/// stylesheet.
/// <para>
/// A physical class is not a compile error and nothing looks wrong in a left-to-right browser,
/// which is exactly why it is checked as text: the defect only appears for a customer reading
/// right to left, and only in the one component that was written without thinking about it.
/// </para>
/// <para>
/// Some physical classes are correct and stay. They fall into two groups, and both are listed
/// below with the reason:
/// </para>
/// <list type="bullet">
/// <item>
/// A component whose own API names a physical side — <c>SheetSide.Left</c>,
/// <c>ToastPosition.TopRight</c>, <c>BadgeDotPosition.BottomLeft</c>. The parameter promises a
/// physical side, so honouring it is the contract.
/// </item>
/// <item>
/// Geometry that is measured in pixels rather than laid out. What remains is small: a thumb
/// centred over its own offset, a lane width, a corner grip that matches a physical resize
/// cursor. The drag and resize maths in these components now reads the direction itself, so the
/// two agree.
/// </item>
/// </list>
/// </summary>
public partial class LogicalPropertyTests
{
    /// <summary>
    /// Files allowed to keep physical direction classes, each with the reason it is exempt.
    /// A new entry here needs a reason that fits one of the two groups above.
    /// </summary>
    private static readonly Dictionary<string, string> Exempt = new(StringComparer.Ordinal)
    {
        // The component's own API names a physical side.
        ["BbSheetContent.razor"] = "SheetSide names a physical side",
        ["BbDrawerContent.razor"] = "DrawerDirection names a physical side",
        ["BbSidebar.razor.cs"] = "SidebarSide names a physical side",
        ["BbSidebarInset.razor.cs"] = "margins pair with SidebarSide",
        ["BbSidebarRail.razor"] = "rail sits against a physical sidebar side",
        ["BbToastProvider.razor"] = "ToastPosition names physical corners",
        ["DialogPositionClasses.cs"] = "DialogPosition names physical sides and corners",
        ["BbBadge.razor.cs"] = "BadgeDotPosition names physical corners",
        ["BbNotificationBadge.razor"] = "BadgeDotPosition names physical corners",
        ["BbTooltipContent.razor"] = "the arrow's edges come from a physical PopoverSide",
        ["BbNavigationMenuIndicator.razor"] = "a rotated arrow's rounded corner is physical",

        // Geometry computed in pixels or percentages from left.
        ["BbScheduler.razor.cs"] = "lane widths are measured in pixels",
        ["BbDock.razor"] = "the corner grip matches a physical resize cursor",
        ["BbSelectionIndicator.razor"] = "the indicator is positioned by measurement",
    };

    /// <summary>
    /// A physical utility that has a logical counterpart. Centring pairs
    /// (<c>left-1/2</c> with <c>-translate-x-1/2</c>) are symmetric and excluded by
    /// <see cref="IsCentring"/> rather than by name.
    /// </summary>
    [GeneratedRegex(
        @"bb:(?:[^\s""]*?:)?!?-?(?<token>ml|mr|pl|pr|border-l|border-r|rounded-tl|rounded-tr|rounded-bl|rounded-br|rounded-l|rounded-r|text-left|text-right|left|right)(?<value>-[^\s""]*)?(?=[""\s]|$)",
        RegexOptions.Compiled)]
    private static partial Regex Physical();

    /// <summary>
    /// <c>left-1/2</c> and <c>left-[50%]</c>, paired with a translate, are how an element is
    /// centred. They point at the same place in both directions, so they are not a direction bug.
    /// The value is read from the match rather than the line, because a long class string is
    /// often split across several lines and the translate then sits on a different one.
    /// </summary>
    private static bool IsCentring(Match match) =>
        match.Groups["value"].Value is "-1/2" or "-[50%]";

    [Fact]
    public void ComponentsUseLogicalDirectionClasses()
    {
        var offenders = new List<string>();

        foreach (var file in SourceTree.ComponentSources)
        {
            if (Exempt.ContainsKey(file.Name))
            {
                continue;
            }

            var lines = File.ReadAllLines(file.FullName);

            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var match in Physical().Matches(lines[i]).Cast<Match>())
                {
                    if (IsCentring(match))
                    {
                        continue;
                    }

                    offenders.Add($"{SourceTree.RelativePath(file)}:{i + 1} {match.Value}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "These utilities are physical, so the layout does not mirror under dir=\"rtl\". Use the "
            + "logical form — ms/me, ps/pe, start/end, border-s/border-e, rounded-s/rounded-e, "
            + "text-start/text-end — or, if the physical side is genuinely correct, add the file to "
            + $"the exemption list with the reason:{Environment.NewLine}"
            + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// A physical CSS property in a hand-authored rule. The Tailwind sweep could not see these
    /// — they are not utility classes — and a one-sided rule is where the first right-to-left
    /// defects turned up after the sweep: a list indent, a blockquote rule, a timeline
    /// alignment.
    /// </summary>
    [GeneratedRegex(
        @"(?<!-)\b(?<prop>margin|padding|border)-(?<side>left|right)\b|\btext-align:\s*(?<align>left|right)\b",
        RegexOptions.Compiled)]
    private static partial Regex PhysicalCssProperty();

    /// <summary>
    /// A declaration is symmetric when the rule sets the same property on both sides, which says
    /// nothing about direction and needs no logical form.
    /// </summary>
    private static bool IsSymmetricPair(string[] lines, int index, Match match)
    {
        var prop = match.Groups["prop"].Value;

        if (prop.Length == 0)
        {
            return false;
        }

        var opposite = prop + "-" + (match.Groups["side"].Value == "left" ? "right" : "left");

        // The pair is written adjacently by convention; scan the whole declaration block instead
        // of the neighbouring line so reordering does not defeat the check.
        var start = index;
        while (start > 0 && !lines[start].Contains('{'))
        {
            start--;
        }

        var end = index;
        while (end < lines.Length - 1 && !lines[end].Contains('}'))
        {
            end++;
        }

        return lines[start..(end + 1)].Any(line => line.Contains(opposite, StringComparison.Ordinal));
    }

    /// <summary>
    /// Hand-authored stylesheets checked alongside the component source: the library's Tailwind
    /// input, and the demo site's, which is the docs users read.
    /// </summary>
    private static IEnumerable<FileInfo> Stylesheets()
    {
        string[] relative =
        [
            Path.Combine("src", "BlazorBlueprint.Components", "wwwroot", "css", "blazorblueprint-input.css"),
            Path.Combine("demos", "BlazorBlueprint.Demo.Shared", "wwwroot", "css", "app-input.css"),
        ];

        return relative
            .Select(path => new FileInfo(Path.Combine(SourceTree.RepoRoot.FullName, path)))
            .Where(file => file.Exists);
    }

    [Fact]
    public void HandWrittenCssUsesLogicalProperties()
    {
        var offenders = new List<string>();

        foreach (var file in Stylesheets())
        {
            var lines = File.ReadAllLines(file.FullName);

            for (var i = 0; i < lines.Length; i++)
            {
                // Tailwind's own generated utility names appear in selectors (.bb\:pl-4); the
                // classes themselves are covered by the source-side check above.
                if (lines[i].TrimStart().StartsWith('.') || lines[i].Contains(@"\:", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var match in PhysicalCssProperty().Matches(lines[i]).Cast<Match>())
                {
                    if (IsSymmetricPair(lines, i, match))
                    {
                        continue;
                    }

                    offenders.Add($"{SourceTree.RelativePath(file)}:{i + 1} {match.Value.Trim()}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "These hand-authored declarations set one physical side, so the rule does not mirror "
            + "under dir=\"rtl\". Use the logical property — margin-inline-start, padding-inline-end, "
            + "border-inline-start, text-align: start — or set both sides, which is symmetric and "
            + $"needs no change:{Environment.NewLine}"
            + string.Join(Environment.NewLine, offenders));
    }

    /// <summary>
    /// An exemption that no longer has a physical class in it is stale, and a stale exemption
    /// silently stops guarding the file it names.
    /// </summary>
    [Fact]
    public void EveryExemptionIsStillNeeded()
    {
        var unused = new List<string>();

        foreach (var (name, reason) in Exempt)
        {
            var files = SourceTree.ComponentSources.Where(f => f.Name == name).ToList();

            if (files.Count == 0)
            {
                unused.Add($"{name} — no such file ({reason})");
                continue;
            }

            var hasPhysical = files.Any(f => File.ReadAllLines(f.FullName)
                .Any(line => Physical().Matches(line).Cast<Match>().Any(m => !IsCentring(m))));

            if (!hasPhysical)
            {
                unused.Add($"{name} — no physical classes left ({reason})");
            }
        }

        Assert.True(unused.Count == 0,
            "These files are exempt from the logical-property rule but no longer need to be. "
            + $"Remove them from the list so the rule guards them again:{Environment.NewLine}"
            + string.Join(Environment.NewLine, unused));
    }
}
