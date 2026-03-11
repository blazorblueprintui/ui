using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DataGrid component.
/// </summary>
public sealed class BbDataGridLabels
{
    [DisallowNull] public string Loading { get; set; } = "Loading...";
    [DisallowNull] public string NoResultsFound { get; set; } = "No results found";
    [DisallowNull] public string NoResultsFilterDescription { get; set; } = "Try adjusting or clearing your filters.";
    [DisallowNull] public string PreviousPage { get; set; } = "Previous page";
    [DisallowNull] public string NextPage { get; set; } = "Next page";
    [DisallowNull] public string ExpandAll { get; set; } = "Expand all";
    [DisallowNull] public string CollapseAll { get; set; } = "Collapse all";
    [DisallowNull] public Func<int, string> SelectAllOnPage { get; set; } = count => $"Select all on this page ({count} items)";
    [DisallowNull] public Func<int, string> SelectAllItems { get; set; } = count => $"Select all {count} items";
    [DisallowNull] public string ClearSelection { get; set; } = "Clear selection";
    [DisallowNull] public string SelectRowsAriaLabel { get; set; } = "Select rows - click to see options";
    [DisallowNull] public string SelectAllRows { get; set; } = "Select all rows";
    [DisallowNull] public string SelectThisRow { get; set; } = "Select this row";
    [DisallowNull] public string ExpandRow { get; set; } = "Expand row";
    [DisallowNull] public string CollapseRow { get; set; } = "Collapse row";
    [DisallowNull] public string Expand { get; set; } = "Expand";
    [DisallowNull] public string Collapse { get; set; } = "Collapse";
    [DisallowNull] public string ExpandGroup { get; set; } = "Expand group";
    [DisallowNull] public string CollapseGroup { get; set; } = "Collapse group";
    [DisallowNull] public Func<string, string> FilterPlaceholder { get; set; } = columnTitle => $"Filter {columnTitle}";
    [DisallowNull] public string PinnedColumnTooltip { get; set; } = "This column is pinned and cannot be moved";
    [DisallowNull] public Func<int, string> ActiveFilters { get; set; } = count => $"{count} active {(count == 1 ? "filter" : "filters")}";
    [DisallowNull] public string ClearAll { get; set; } = "Clear all";
    [DisallowNull] public Func<int, int, int, string> ShowingRange { get; set; } = (start, end, total) => $"Showing {start}\u2013{end} of {total}";
    [DisallowNull] public Func<int, int, string> RowsSelected { get; set; } = (selected, total) => $"{selected} of {total} row(s) selected";
    [DisallowNull] public Func<int, string> GroupItemCount { get; set; } = count => $"({count} items)";
    [DisallowNull] public string CountLabel { get; set; } = "Count";
    [DisallowNull] public string SumLabel { get; set; } = "Sum";
    [DisallowNull] public string AverageLabel { get; set; } = "Avg";
    [DisallowNull] public string MinLabel { get; set; } = "Min";
    [DisallowNull] public string MaxLabel { get; set; } = "Max";
    [DisallowNull] public string FilterColumnEnterValue { get; set; } = "Enter value...";
    [DisallowNull] public string FilterColumnMin { get; set; } = "Min";
    [DisallowNull] public string FilterColumnAnd { get; set; } = "and";
    [DisallowNull] public string FilterColumnMax { get; set; } = "Max";
    [DisallowNull] public string FilterColumnAmount { get; set; } = "Amount";
    [DisallowNull] public string FilterColumnPickDate { get; set; } = "Pick a date";
    [DisallowNull] public string FilterColumnSelectValues { get; set; } = "Select values...";
    [DisallowNull] public string FilterColumnSelectValue { get; set; } = "Select value...";
    [DisallowNull] public string FilterColumnClear { get; set; } = "Clear";
    [DisallowNull] public string FilterColumnApply { get; set; } = "Apply";
}
