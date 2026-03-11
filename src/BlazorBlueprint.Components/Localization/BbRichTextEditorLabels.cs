using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the RichTextEditor component.
/// </summary>
public sealed class BbRichTextEditorLabels
{
    // Heading options
    [DisallowNull] public string Normal { get; set; } = "Normal";
    [DisallowNull] public string Heading1 { get; set; } = "Heading 1";
    [DisallowNull] public string Heading2 { get; set; } = "Heading 2";
    [DisallowNull] public string Heading3 { get; set; } = "Heading 3";

    // Formatting tooltips
    [DisallowNull] public string Bold { get; set; } = "Bold (Ctrl+B)";
    [DisallowNull] public string Italic { get; set; } = "Italic (Ctrl+I)";
    [DisallowNull] public string Underline { get; set; } = "Underline (Ctrl+U)";
    [DisallowNull] public string Strikethrough { get; set; } = "Strikethrough";
    [DisallowNull] public string BulletList { get; set; } = "Bullet List";
    [DisallowNull] public string NumberedList { get; set; } = "Numbered List";
    [DisallowNull] public string InsertLink { get; set; } = "Insert Link";
    [DisallowNull] public string Blockquote { get; set; } = "Blockquote";
    [DisallowNull] public string CodeBlock { get; set; } = "Code Block";

    // Link dialog
    [DisallowNull] public string EditLink { get; set; } = "Edit Link";
    [DisallowNull] public string InsertLinkTitle { get; set; } = "Insert Link";
    [DisallowNull] public string EditLinkDescription { get; set; } = "Update the URL or remove the link.";
    [DisallowNull] public string InsertLinkDescription { get; set; } = "Enter the URL for the selected text.";
    [DisallowNull] public string RemoveLink { get; set; } = "Remove Link";
    [DisallowNull] public string Cancel { get; set; } = "Cancel";
    [DisallowNull] public string Update { get; set; } = "Update";
    [DisallowNull] public string Insert { get; set; } = "Insert";
}
