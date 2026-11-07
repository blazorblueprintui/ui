#pragma warning disable IDE0022
using System.IO;
using System.Text.RegularExpressions;

namespace BlazorBlueprint.Tests.Conventions;

/// <summary>
/// Guards the two CSS fixes that make the exit animations usable (#584): a global
/// <c>animation-fill-mode: both</c> so a finished exit stays invisible until unmount, and a native
/// <c>&lt;dialog&gt;</c> backdrop that fades out alongside the dialog instead of vanishing.
/// </summary>
public class OverlayAnimationCssTests
{
    private static string CssPath => Path.Combine(SourceTree.RepoRoot.FullName, "src", "BlazorBlueprint.Components", "wwwroot", "css", "blazorblueprint-input.css");

    private static string Css => File.ReadAllText(CssPath);

    [Fact]
    public void DataStateElementsHoldTheirFinalKeyframe()
    {
        // tw-animate compiles animate-in/animate-out with fill-mode none. Without `both`, an overlay
        // that finishes its exit early snaps back to full visibility, and a component whose unmount
        // waits on the exit pops back fully visible for the unmount render's round-trip on Server.
        Assert.Matches(
            new Regex(@"\[data-state=""open""\],\s*\n\[data-state=""closed""\]\s*\{\s*\n\s*animation-fill-mode:\s*both;", RegexOptions.Singleline),
            Css);
    }

    [Fact]
    public void NativeDialogBackdropFadesOutWithTheDialog()
    {
        var css = Css;

        // The scrim must transition on both open and closed states, and end transparent when closed.
        Assert.Contains("dialog[data-state]::backdrop", css, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"dialog\[data-state\]::backdrop\s*\{[^}]*transition:\s*background", RegexOptions.Singleline), css);
        Assert.Matches(new Regex(@"dialog\[data-state=""closed""\]::backdrop\s*\{[^}]*background:\s*rgb\(0 0 0 / 0\)", RegexOptions.Singleline), css);
        Assert.Matches(new Regex(@"dialog\[data-state=""closed""\]::backdrop\s*\{[^}]*backdrop-filter:\s*none", RegexOptions.Singleline), css);
    }

    [Fact]
    public void ReducedMotionStillSuppressesTheExitAnimations()
    {
        // The unlayered prefers-reduced-motion rule forces animation durations to ~0, so waitForExit
        // resolves at once and the overlays leave without playing their exit.
        Assert.Matches(new Regex(@"@media\s*\(\s*prefers-reduced-motion:\s*reduce\s*\)\s*\{[^}]*animation-duration:\s*0\.01ms\s*!important", RegexOptions.Singleline), Css);
    }
}
