namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the TagInput component.
/// </summary>
public class BbTagInputLabels
{
    public string Placeholder { get; set; } = "Add tag...";
    public Func<string, string> RemoveTag { get; set; } = tag => $"Remove {tag}";
    public string ClearAllTags { get; set; } = "Clear all tags";
    public string TagSuggestions { get; set; } = "Tag suggestions";
}
