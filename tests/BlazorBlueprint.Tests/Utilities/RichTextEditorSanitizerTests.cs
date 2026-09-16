using System.Reflection;
using BlazorBlueprint.Components;
using Ganss.Xss;
using Xunit;

namespace BlazorBlueprint.Tests.Utilities;

/// <summary>
/// <c>BbRichTextEditor</c> runs every incoming <c>Value</c> through an <c>HtmlSanitizer</c>.
/// Its defaults would strip what Quill needs to round-trip a document: the checklist marker
/// and data-URL images. These pin the exceptions that were added, and that the exceptions stay
/// as narrow as intended.
/// </summary>
public class RichTextEditorSanitizerTests
{
    private static readonly HtmlSanitizer Sanitizer = ResolveSanitizer();

    private static HtmlSanitizer ResolveSanitizer()
    {
        var field = typeof(BbRichTextEditor).GetField("Sanitizer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.False(field is null, "BbRichTextEditor no longer has a private static `Sanitizer` field; update this test to reach the editor's sanitizer.");
        return (HtmlSanitizer)field!.GetValue(null)!;
    }

    [Fact]
    public void KeepsChecklistState() =>
        Assert.Contains("data-list=\"checked\"", Sanitizer.Sanitize("<ul><li data-list=\"checked\">done</li></ul>"), StringComparison.Ordinal);

    [Fact]
    public void KeepsDataUrlImages()
    {
        const string html = "<p><img src=\"data:image/png;base64,iVBORw0KGgo=\"></p>";
        Assert.Contains("src=\"data:image/png;base64,iVBORw0KGgo=\"", Sanitizer.Sanitize(html), StringComparison.Ordinal);
    }

    [Fact]
    public void DropsNonImageDataUrls() =>
        Assert.DoesNotContain("data:", Sanitizer.Sanitize("<p><img src=\"data:text/html;base64,PHNjcmlwdD4=\"></p>"), StringComparison.Ordinal);

    [Fact]
    public void DropsDataUrlLinks() =>
        Assert.DoesNotContain("data:", Sanitizer.Sanitize("<a href=\"data:image/svg+xml,%3Csvg%3E%3C/svg%3E\">x</a>"), StringComparison.Ordinal);

    [Fact]
    public void KeepsTablesAlignmentAndColour()
    {
        const string html = "<table><tbody><tr><td>a</td></tr></tbody></table>"
            + "<p style=\"text-align: center\"><span style=\"color: rgb(220, 38, 38); background-color: rgb(254, 240, 138)\">x</span> <code>y</code></p>";
        var result = Sanitizer.Sanitize(html);

        Assert.Contains("<td>a</td>", result, StringComparison.Ordinal);
        Assert.Contains("text-align: center", result, StringComparison.Ordinal);
        // AngleSharp re-serialises colours (rgb → rgba with alpha), so match the channels, not the spelling.
        Assert.Matches(@"[^-]color:\s*rgba?\(220,\s*38,\s*38(,\s*1)?\)", result);
        Assert.Matches(@"background-color:\s*rgba?\(254,\s*240,\s*138(,\s*1)?\)", result);
        Assert.Contains("<code>y</code>", result, StringComparison.Ordinal);
    }

    [Fact]
    public void StillStripsScript() =>
        Assert.DoesNotContain("<script", Sanitizer.Sanitize("<p>x</p><script>alert(1)</script><img src=x onerror=alert(1)>"), StringComparison.OrdinalIgnoreCase);
}
