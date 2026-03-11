using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the Combobox component.
/// </summary>
public sealed class BbComboboxLabels
{
    [DisallowNull] public string EmptyMessage { get; set; } = "No results found.";
    [DisallowNull] public string Placeholder { get; set; } = "Select an option...";
    [DisallowNull] public string SearchPlaceholder { get; set; } = "Search...";
}
