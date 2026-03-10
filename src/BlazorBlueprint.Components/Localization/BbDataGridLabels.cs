namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DataGrid component.
/// </summary>
public class BbDataGridLabels
{
    public string Loading { get; set; } = "Loading...";
    public string NoResultsFound { get; set; } = "No results found";
    public string NoResultsFilterDescription { get; set; } = "Try adjusting or clearing your filters.";
    public string PreviousPage { get; set; } = "Previous page";
    public string NextPage { get; set; } = "Next page";
    public string ExpandAll { get; set; } = "Expand all";
    public string CollapseAll { get; set; } = "Collapse all";
    public Func<int, string> SelectAllOnPage { get; set; } = count => $"Select all on this page ({count} items)";
    public Func<int, string> SelectAllItems { get; set; } = count => $"Select all {count} items";
    public string ClearSelection { get; set; } = "Clear selection";
    public string SelectRowsAriaLabel { get; set; } = "Select rows - click to see options";
    public string SelectAllRows { get; set; } = "Select all rows";
    public string SelectThisRow { get; set; } = "Select this row";
    public string ExpandRow { get; set; } = "Expand row";
    public string CollapseRow { get; set; } = "Collapse row";
    public string Expand { get; set; } = "Expand";
    public string Collapse { get; set; } = "Collapse";
    public string ExpandGroup { get; set; } = "Expand group";
    public string CollapseGroup { get; set; } = "Collapse group";
    public Func<string, string> FilterPlaceholder { get; set; } = columnTitle => $"Filter {columnTitle}";
    public string PinnedColumnTooltip { get; set; } = "This column is pinned and cannot be moved";
    public Func<int, string> ActiveFilters { get; set; } = count => $"{count} active {(count == 1 ? "filter" : "filters")}";
    public string ClearAll { get; set; } = "Clear all";
    public Func<int, int, int, string> ShowingRange { get; set; } = (start, end, total) => $"Showing {start}\u2013{end} of {total}";
    public Func<int, int, string> RowsSelected { get; set; } = (selected, total) => $"{selected} of {total} row(s) selected";
    public Func<int, string> GroupItemCount { get; set; } = count => $"({count} items)";
    public string CountLabel { get; set; } = "Count";
    public string SumLabel { get; set; } = "Sum";
    public string AverageLabel { get; set; } = "Avg";
    public string MinLabel { get; set; } = "Min";
    public string MaxLabel { get; set; } = "Max";
    public string FilterColumnEnterValue { get; set; } = "Enter value...";
    public string FilterColumnMin { get; set; } = "Min";
    public string FilterColumnAnd { get; set; } = "and";
    public string FilterColumnMax { get; set; } = "Max";
    public string FilterColumnAmount { get; set; } = "Amount";
    public string FilterColumnPickDate { get; set; } = "Pick a date";
    public string FilterColumnSelectValues { get; set; } = "Select values...";
    public string FilterColumnSelectValue { get; set; } = "Select value...";
    public string FilterColumnClear { get; set; } = "Clear";
    public string FilterColumnApply { get; set; } = "Apply";
}
