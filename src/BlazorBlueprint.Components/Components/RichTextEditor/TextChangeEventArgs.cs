using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components.RichTextEditor;

/// <summary>
/// Event arguments for text change events in the rich text editor.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "EventArgs suffix is intentional for this DTO")]
public class TextChangeEventArgs
{
    /// <summary>
    /// Gets or sets the delta representing the changes made.
    /// </summary>
    public string? Delta { get; set; }

    /// <summary>
    /// Gets or sets the delta representing the previous content.
    /// </summary>
    public string? OldDelta { get; set; }

    /// <summary>
    /// Gets or sets the source of the change (user, api, silent).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the current HTML content of the editor.
    /// </summary>
    public string? Html { get; set; }

    /// <summary>
    /// Gets or sets the current plain text content of the editor.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the length of the editor content.
    /// </summary>
    public int Length { get; set; }
}
