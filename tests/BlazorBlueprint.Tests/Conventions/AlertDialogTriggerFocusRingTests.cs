using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Regression guard for the last of #459's "Overlay triggers" group: <c>BbDialogTrigger</c>,
/// <c>BbSheetTrigger</c> and <c>BbPopoverTrigger</c> were fixed in #508, which left
/// <c>BbAlertDialogTrigger</c> - it renders a native <c>&lt;button&gt;</c> (via the Dialog primitive
/// it wraps) with no focus styling at all, neither <c>outline-none</c> nor a replacement.
/// <see cref="FocusIndicatorTests"/> only fires once <c>outline-none</c> is present, so it never
/// caught this: focus stayed visible via the browser's own outline, just inconsistent with the
/// themed ring every other standalone trigger now uses.
/// </summary>
public class AlertDialogTriggerFocusRingTests
{
    private static readonly Regex StandaloneRing = new(
        @"focus-visible:outline-none\s+focus-visible:ring-2\s+focus-visible:ring-ring\s+focus-visible:ring-offset-2",
        RegexOptions.Compiled);

    [Fact]
    public void ComponentsLayerAppliesTheStandaloneFocusRing()
    {
        var text = SourceTree.ByComponent()
            .Where(c => c.Component == "BbAlertDialogTrigger")
            .Select(c => c.Text)
            .SingleOrDefault();

        Assert.False(text is null, "Expected a component named BbAlertDialogTrigger under "
            + "src/BlazorBlueprint.Components or src/BlazorBlueprint.Primitives - has it been renamed or moved?");

        Assert.True(StandaloneRing.IsMatch(text!),
            "BbAlertDialogTrigger should apply the library's standard standalone-control focus ring "
            + "(focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2) "
            + "to the native button it renders, matching BbDialogTrigger/BbSheetTrigger/BbPopoverTrigger (#508).");
    }
}
