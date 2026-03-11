using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the MultiSelect component.
/// </summary>
public sealed class BbMultiSelectLabels
{
    [DisallowNull] public string EmptyMessage { get; set; } = "No results found.";
    [DisallowNull] public string Placeholder { get; set; } = "Select items...";
    [DisallowNull] public string SearchPlaceholder { get; set; } = "Search...";
    [DisallowNull] public string SelectAll { get; set; } = "Select All";
    [DisallowNull] public string Clear { get; set; } = "Clear";
    [DisallowNull] public string Close { get; set; } = "Close";
}
