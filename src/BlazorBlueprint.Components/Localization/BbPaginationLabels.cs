using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the Pagination component.
/// </summary>
public sealed class BbPaginationLabels
{
    [DisallowNull] public string Pagination { get; set; } = "Pagination";
    [DisallowNull] public string Previous { get; set; } = "Previous";
    [DisallowNull] public string Next { get; set; } = "Next";
    [DisallowNull] public string MorePages { get; set; } = "More pages";
    [DisallowNull] public string GoToFirstPage { get; set; } = "Go to first page";
    [DisallowNull] public string GoToLastPage { get; set; } = "Go to last page";
    [DisallowNull] public string RowsPerPage { get; set; } = "Rows per page";
    [DisallowNull] public string ShowingFormat { get; set; } = "Showing {0}-{1} of {2}";
    [DisallowNull] public string PageFormat { get; set; } = "Page {0} of {1}";
    [DisallowNull] public string NoItems { get; set; } = "No items";
}
