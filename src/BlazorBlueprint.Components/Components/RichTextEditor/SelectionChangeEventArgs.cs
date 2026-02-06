using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components.RichTextEditor;

/// <summary>
/// Event arguments for selection change events in the rich text editor.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "EventArgs suffix is intentional for this DTO")]
public class SelectionChangeEventArgs
{
    /// <summary>
    /// Gets or sets the current selection range, or null if the editor lost focus.
    /// </summary>
    public EditorRange? Range { get; set; }

    /// <summary>
    /// Gets or sets the previous selection range.
    /// </summary>
    public EditorRange? OldRange { get; set; }

    /// <summary>
    /// Gets or sets the source of the change (user, api, silent).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the formatting at the current selection.
    /// </summary>
    public Dictionary<string, object?>? Format { get; set; }
}
