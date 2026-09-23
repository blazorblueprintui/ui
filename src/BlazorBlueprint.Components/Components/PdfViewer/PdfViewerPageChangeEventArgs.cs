using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Event arguments for the <see cref="BbPdfViewer.OnPageChanged"/> event.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Committing to the 'EventArgs' suffix for consistency with the framework convention.")]
public sealed class PdfViewerPageChangeEventArgs
{
    /// <summary>
    /// Gets or sets the page now being displayed (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages in the loaded document.
    /// </summary>
    public int PageCount { get; set; }
}