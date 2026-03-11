using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the TagInput component.
/// </summary>
public sealed class BbTagInputLabels
{
    [DisallowNull] public string Placeholder { get; set; } = "Add tag...";
    [DisallowNull] public Func<string, string> RemoveTag { get; set; } = tag => $"Remove {tag}";
    [DisallowNull] public string ClearAllTags { get; set; } = "Clear all tags";
    [DisallowNull] public string TagSuggestions { get; set; } = "Tag suggestions";
}
