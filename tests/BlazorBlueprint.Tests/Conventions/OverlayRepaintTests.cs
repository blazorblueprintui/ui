using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Guards the overlay roots against the desync behind
/// <see href="https://github.com/blazorblueprintui/ui/issues/437">#437</see>.
/// <para>
/// Each overlay root subscribes to its context and reconciles two things in
/// <c>HandleContextStateChanged</c>: telling the consumer the open state moved, and repainting its
/// own subtree. Those are not the same decision, and the bug was treating them as one — a single
/// <c>if</c> wrapped both, so when the notify was skipped the repaint was skipped with it.
/// </para>
/// <para>
/// It went wrong twice over. The guard compared against <c>_state.Value</c>, which in controlled
/// mode is <c>ControlledValue</c> — only refreshed in <c>OnParametersSet</c>, and therefore only
/// when the consumer re-renders. <c>BbDataGrid</c> suppresses that via <c>ShouldRender</c>, so the
/// value went stale, the comparison came out equal, and both the callback and the repaint were
/// skipped forever: the filter button kept rendering <c>aria-expanded="true"</c> with
/// <c>pointer-events: none</c> and could never be clicked again.
/// </para>
/// <para>
/// These are text assertions rather than behavioural ones for the reason
/// <see cref="SourceTree"/> gives: the failure is a brace in the wrong place. Every line still
/// runs and the logic still type-checks — only the nesting is wrong, which rendered output at the
/// primitive level cannot show without a consumer that suppresses re-rendering.
/// </para>
/// </summary>
public partial class OverlayRepaintTests
{
    /// <summary>
    /// The overlay roots that own a controllable open state and a context subscription.
    /// <c>BbTooltip</c> is absent deliberately — it does not repaint from this path at all.
    /// </summary>
    private static readonly string[] OverlayRoots =
    [
        "BbPopover",
        "BbDropdownMenu",
        "BbHoverCard",
        "BbDialog",
        "BbSheet",
    ];

    [Fact]
    public void OverlayRootsRepaintWheneverContextStateMoves()
    {
        var offenders = new List<string>();

        foreach (var (component, handler) in Handlers())
        {
            // Strip the guarded block; a StateHasChanged that survives is one that always runs.
            var outsideGuard = GuardedBlock().Replace(handler, string.Empty);

            if (!outsideGuard.Contains("StateHasChanged()", StringComparison.Ordinal))
            {
                offenders.Add(component);
            }
        }

        Assert.True(offenders.Count == 0,
            $"{string.Join(", ", offenders)}: StateHasChanged() only runs inside the notify guard. "
            + "The trigger's aria-expanded and pointer-events, and whether the content is mounted, "
            + "all derive from context state — so the subtree must repaint whenever that state "
            + "moves, including when the consumer's controlled value did not. Move the call out "
            + "of the if. See #437.");
    }

    [Fact]
    public void OverlayRootsDoNotMeasureChangesAgainstTheControlledValue()
    {
        var offenders = new List<string>();

        foreach (var (component, handler) in Handlers())
        {
            if (handler.Contains("_state.Value != newOpenState", StringComparison.Ordinal))
            {
                offenders.Add(component);
            }
        }

        Assert.True(offenders.Count == 0,
            $"{string.Join(", ", offenders)}: the notify guard compares against _state.Value. "
            + "In controlled mode that is ControlledValue, which only refreshes in OnParametersSet "
            + "— so any consumer suppressing its re-render (ShouldRender) leaves it stale and the "
            + "callback silently stops firing. Compare against the state last reported instead. "
            + "See #437.");
    }

    [Fact]
    public void EveryOverlayRootIsActuallyBeingChecked()
    {
        // Without this, renaming a component or its handler would quietly empty the two tests
        // above and let them pass by inspecting nothing at all.
        var found = Handlers().Select(h => h.Component).ToList();

        var missing = OverlayRoots.Except(found, StringComparer.Ordinal).ToList();

        Assert.True(missing.Count == 0,
            $"No HandleContextStateChanged found for: {string.Join(", ", missing)}. "
            + "Either the component was renamed or the handler was — update OverlayRoots, or the "
            + "checks in this class are silently inspecting nothing.");
    }

    /// <summary>The body of each overlay root's <c>HandleContextStateChanged</c>.</summary>
    private static List<(string Component, string Handler)> Handlers()
    {
        var results = new List<(string, string)>();

        foreach (var (component, text) in SourceTree.ByComponent())
        {
            if (!OverlayRoots.Contains(component, StringComparer.Ordinal))
            {
                continue;
            }

            var match = HandlerBody().Match(text);
            if (match.Success)
            {
                results.Add((component, match.Value));
            }
        }

        return results;
    }

    /// <summary>
    /// From the handler signature to the closing brace at method indentation — enough to capture
    /// the body without needing to balance braces.
    /// </summary>
    [GeneratedRegex(@"private void HandleContextStateChanged\(\)\r?\n\s*\{.*?\r?\n    \}",
        RegexOptions.Singleline)]
    private static partial Regex HandlerBody();

    /// <summary>The <c>if</c> block that decides whether to notify the consumer.</summary>
    [GeneratedRegex(@"if \([^)]*!= newOpenState\)\r?\n\s*\{.*?\r?\n        \}",
        RegexOptions.Singleline)]
    private static partial Regex GuardedBlock();
}
