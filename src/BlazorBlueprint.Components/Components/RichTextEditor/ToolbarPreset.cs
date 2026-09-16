namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the preset toolbar configurations for the rich text editor.
/// </summary>
public enum ToolbarPreset
{
    /// <summary>
    /// No toolbar displayed.
    /// </summary>
    None,

    /// <summary>
    /// Bold, italic, underline, bullet and numbered lists.
    /// </summary>
    Simple,

    /// <summary>
    /// <see cref="Simple"/> plus undo/redo, headings, strikethrough, checklist and links.
    /// </summary>
    Standard,

    /// <summary>
    /// <see cref="Standard"/> plus inline code, text alignment, text colour and highlight,
    /// images, blockquote, code block and tables.
    /// </summary>
    Full,

    /// <summary>
    /// Custom toolbar - use ToolbarContent to provide custom toolbar markup.
    /// </summary>
    Custom
}
