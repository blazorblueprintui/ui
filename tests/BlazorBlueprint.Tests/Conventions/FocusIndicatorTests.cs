using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Guards against a component removing the browser's focus outline without drawing a replacement.
/// <para>
/// <c>outline-none</c> suppresses the user agent's focus ring. Without something in its place a
/// focused control is indistinguishable from an unfocused one, which fails
/// <see href="https://www.w3.org/WAI/WCAG21/Understanding/focus-visible.html">WCAG 2.4.7</see>.
/// The whole text-input family shipped that way until #457 — reported in discussion #355, where
/// three people said it decided a library choice against this one.
/// </para>
/// <para>
/// The rule deliberately accepts any recognised affordance, not just a ring: menu items indicate
/// focus with a background change, which is the correct pattern for a roving-focus list.
/// </para>
/// </summary>
public class FocusIndicatorTests
{
    private static readonly Regex RemovesOutline =
        new(@"(?:focus(?:-visible)?:)?outline-none", RegexOptions.Compiled);

    /// <summary>
    /// A ring, a ring-coloured border, or a background/text change on focus or on the
    /// highlighted/selected state of a list item.
    /// </summary>
    private static readonly Regex HasIndicator = new(
        @"focus(?:-visible|-within)?:(?:ring-[1-9]|border-ring|bg-|text-)"
        + @"|data-\[(?:focused|highlighted)[^\]]*\]:(?:bg-|text-|ring-)",
        RegexOptions.Compiled);

    /// <summary>
    /// Options in a listbox drive their own selected styling, so <c>aria-selected</c> is itself
    /// evidence of an affordance.
    /// </summary>
    private static readonly Regex ManagesSelection =
        new(@"aria-selected|data-\[state=selected\]", RegexOptions.Compiled);

    /// <summary>
    /// Overlay panels are focused programmatically when they open. A ring drawn round the whole
    /// panel on every open would be wrong, so <c>outline-none</c> is correct for these by suffix.
    /// </summary>
    private static readonly string[] ContainerSuffixes =
        ["Content", "Overlay", "Portal", "Provider", "Host"];

    /// <summary>
    /// Components exempted by name, each for a stated reason. Keep this list short — an allowlist
    /// nobody trusts is an allowlist that gets muted. Entries marked #459 are real gaps awaiting
    /// that issue; they are listed rather than silently skipped so removing them is a visible edit.
    /// </summary>
    private static readonly Dictionary<string, string> Allowed = new(StringComparer.Ordinal)
    {
        ["BbInputGroup"] = "Wrapper element; the inner input carries the ring.",
        ["BbSidebarInset"] = "Layout container for page content, not a control.",
        ["BbDashboardWidget"] = "Widget shell; the focusable controls inside it carry their own.",
        ["BbDataGrid"] = "Grid container; header cells and rows manage their own focus.",
        ["BbAttachmentTrigger"] = "Known gap, tracked in #459.",
        ["BbCommandInput"] = "Known gap, tracked in #459.",
    };

    [Fact]
    public void ComponentsThatRemoveTheOutlineProvideAFocusIndicator()
    {
        var violations = new List<string>();

        foreach (var (component, text) in SourceTree.ByComponent())
        {
            if (!RemovesOutline.IsMatch(text))
            {
                continue;
            }

            if (HasIndicator.IsMatch(text) || ManagesSelection.IsMatch(text))
            {
                continue;
            }

            if (ContainerSuffixes.Any(s => component.EndsWith(s, StringComparison.Ordinal)))
            {
                continue;
            }

            if (Allowed.ContainsKey(component))
            {
                continue;
            }

            violations.Add(component);
        }

        Assert.True(violations.Count == 0, BuildMessage(violations));
    }

    /// <summary>
    /// An allowlist entry that no longer applies is worse than none: it hides a regression behind a
    /// stale exemption. If a component here has since gained an indicator, delete its entry.
    /// </summary>
    [Fact]
    public void AllowlistHasNoStaleEntries()
    {
        var byComponent = SourceTree.ByComponent()
            .ToDictionary(x => x.Component, x => x.Text, StringComparer.Ordinal);

        var stale = Allowed.Keys
            .Where(c => !byComponent.TryGetValue(c, out var text)
                        || !RemovesOutline.IsMatch(text)
                        || HasIndicator.IsMatch(text)
                        || ManagesSelection.IsMatch(text))
            .ToList();

        Assert.True(stale.Count == 0,
            "These allowlist entries no longer need an exemption — the component has gained a focus "
            + "indicator, or no longer removes the outline, or no longer exists. Remove them from "
            + $"{nameof(Allowed)}:{Environment.NewLine}  " + string.Join($"{Environment.NewLine}  ", stale));
    }

    private static string BuildMessage(IReadOnlyCollection<string> violations)
    {
        var message = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture,
                $"{violations.Count} component(s) remove the focus outline without replacing it.")
            .AppendLine()
            .AppendLine("A focused control then looks identical to an unfocused one, which fails WCAG 2.4.7.")
            .AppendLine()
            .AppendLine("Add the library's ring:")
            .AppendLine("  focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2")
            .AppendLine()
            .AppendLine("Drop ring-offset-2 for controls that sit directly under a label — form rows")
            .AppendLine("leave a 3px gap and an offset ring extends 4px, so it overlaps the label.")
            .AppendLine()
            .AppendLine("A background change on focus counts too, and is the right pattern for menu items.")
            .AppendLine("If the component genuinely needs no indicator, add it to the allowlist with a reason.")
            .AppendLine();

        foreach (var violation in violations)
        {
            message.AppendLine("  " + violation);
        }

        return message.ToString();
    }
}
