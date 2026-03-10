namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DataTable component.
/// </summary>
public class BbDataTableLabels
{
    public string Loading { get; set; } = "Loading...";
    public string NoResultsFound { get; set; } = "No results found";
    public string SelectRowsAriaLabel { get; set; } = "Select rows - click to see options";
    public Func<int, string> SelectAllOnPage { get; set; } = count => $"Select all on this page ({count} items)";
    public Func<int, string> SelectAllItems { get; set; } = count => $"Select all {count} items";
    public string ClearSelection { get; set; } = "Clear selection";
    public string SelectAllRows { get; set; } = "Select all rows";
    public string SelectThisRow { get; set; } = "Select this row";
    public string Search { get; set; } = "Search...";
    public string Columns { get; set; } = "Columns";
    public string ToggleColumns { get; set; } = "Toggle columns";
    public string Filter { get; set; } = "Filter";
    public string FilterColumns { get; set; } = "Filter columns";
}
