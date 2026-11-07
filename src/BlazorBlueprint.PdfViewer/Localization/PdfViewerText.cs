using BlazorBlueprint.Components;

namespace BlazorBlueprint.PdfViewer;

/// <summary>
/// Reads a string from the application's localiser, falling back to the viewer's own English where
/// the localiser answers with the key (its answer for a key it has never heard of).
/// </summary>
/// <param name="localizer">The application's localiser.</param>
internal sealed class PdfViewerText(IBbLocalizer localizer)
{
    public string this[string key]
    {
        get
        {
            var found = localizer[key];
            return string.Equals(found, key, StringComparison.Ordinal) ? PdfViewerLocalization.English(key) : found;
        }
    }
}
