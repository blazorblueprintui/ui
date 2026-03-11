using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DataTable component.
/// </summary>
public sealed class BbDataTableLabels
{
    [DisallowNull] public string Loading { get; set; } = "Loading...";
    [DisallowNull] public string NoResultsFound { get; set; } = "No results found";
    [DisallowNull] public string SelectRowsAriaLabel { get; set; } = "Select rows - click to see options";
    [DisallowNull] public Func<int, string> SelectAllOnPage { get; set; } = count => $"Select all on this page ({count} items)";
    [DisallowNull] public Func<int, string> SelectAllItems { get; set; } = count => $"Select all {count} items";
    [DisallowNull] public string ClearSelection { get; set; } = "Clear selection";
    [DisallowNull] public string SelectAllRows { get; set; } = "Select all rows";
    [DisallowNull] public string SelectThisRow { get; set; } = "Select this row";
    [DisallowNull] public string Search { get; set; } = "Search...";
    [DisallowNull] public string Columns { get; set; } = "Columns";
    [DisallowNull] public string ToggleColumns { get; set; } = "Toggle columns";
    [DisallowNull] public string Filter { get; set; } = "Filter";
    [DisallowNull] public string FilterColumns { get; set; } = "Filter columns";
}
