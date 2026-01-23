namespace BlazorUI.Components.RichTextEditor;

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
    /// Simple toolbar with basic formatting (bold, italic, underline, lists).
    /// </summary>
    Simple,

    /// <summary>
    /// Standard toolbar with common formatting options.
    /// </summary>
    Standard,

    /// <summary>
    /// Full toolbar with all available formatting options.
    /// </summary>
    Full,

    /// <summary>
    /// Custom toolbar - use ToolbarContent to provide custom toolbar markup.
    /// </summary>
    Custom
}
