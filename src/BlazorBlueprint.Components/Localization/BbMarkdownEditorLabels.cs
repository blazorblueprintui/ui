using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the MarkdownEditor component.
/// </summary>
public sealed class BbMarkdownEditorLabels
{
    [DisallowNull] public string SelectHeadingLevel { get; set; } = "Select heading level";
    [DisallowNull] public string Bold { get; set; } = "Bold (Ctrl+B)";
    [DisallowNull] public string Italic { get; set; } = "Italic (Ctrl+I)";
    [DisallowNull] public string Underline { get; set; } = "Underline (Ctrl+U)";
    [DisallowNull] public string BulletList { get; set; } = "Bullet list";
    [DisallowNull] public string NumberedList { get; set; } = "Numbered list";
}
