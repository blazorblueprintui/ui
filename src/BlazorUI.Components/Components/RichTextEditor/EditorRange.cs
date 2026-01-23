namespace BlazorUI.Components.RichTextEditor;

/// <summary>
/// Represents a selection range in the rich text editor.
/// </summary>
public class EditorRange
{
    /// <summary>
    /// Gets or sets the starting index of the selection.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the length of the selection.
    /// </summary>
    public int Length { get; set; }
}
