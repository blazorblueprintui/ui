using BlazorBlueprint.Components;

namespace BlazorBlueprint.PdfViewer;

/// <summary>
/// Every string the PDF viewer shows, and its English default.
/// </summary>
/// <remarks>
/// Resolved through <see cref="IBbLocalizer"/> under keys beginning <c>PdfViewer.</c>: set a key and
/// the viewer picks it up; set none and it shows the English here. <c>DefaultBbLocalizer</c> cannot
/// know an add-on package's keys, so a miss falls back to this table rather than to the key.
/// <example>
/// <code>
/// builder.Services.AddBlazorBlueprintComponents(localizer =>
/// {
///     localizer.Set("PdfViewer.NextPage", "Page suivante");
/// });
/// </code>
/// </example>
/// </remarks>
public static class PdfViewerLocalization
{
    /// <summary>The prefix every PDF viewer key begins with.</summary>
    public const string Prefix = "PdfViewer.";

    private static readonly Dictionary<string, string> DefaultStrings = new(StringComparer.Ordinal)
    {
        ["PdfViewer.AriaLabel"] = "PDF document",
        ["PdfViewer.CurrentPage"] = "Current page",
        ["PdfViewer.Download"] = "Download PDF",
        ["PdfViewer.FitToWidth"] = "Fit to width",
        ["PdfViewer.Loading"] = "Loading PDF…",
        ["PdfViewer.LoadFailed"] = "Unable to load the PDF document.",
        ["PdfViewer.NextPage"] = "Next page",
        ["PdfViewer.PreviousPage"] = "Previous page",
        ["PdfViewer.ZoomIn"] = "Zoom in",
        ["PdfViewer.ZoomOut"] = "Zoom out",
    };

    /// <summary>
    /// Gets every key the viewer uses, with its English default.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Defaults => DefaultStrings;

    /// <summary>
    /// Gets the English default for a key, or the key itself when the viewer has no such key.
    /// </summary>
    /// <param name="key">The localization key, such as <c>PdfViewer.NextPage</c>.</param>
    /// <returns>The English text.</returns>
    public static string English(string key) =>
        DefaultStrings.TryGetValue(key, out var value) ? value : key;
}
