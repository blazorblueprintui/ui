using Microsoft.Extensions.Logging;

namespace BlazorBlueprint.Components;

/// <summary>
/// Source-generated log messages for the DataGrid. Declared outside the generic grid component
/// so the generated methods do not need to be nested inside a generic containing type.
/// </summary>
internal static partial class DataGridLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "DataGrid is grouped by column '{ColumnId}' while using Virtualize with an ItemsProvider, " +
                  "a combination that cannot group client-side, so no rows will render. Supply a " +
                  "GroupedItemsProvider to group server-side, or turn off Virtualize.")]
    public static partial void GroupingUnsupportedInVirtualizedProvider(ILogger logger, string columnId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "DataGrid export wrote only the page currently loaded, because the grid uses an " +
                  "ItemsProvider and never holds more than one page. Set ExportScope to CurrentPage " +
                  "to make that explicit, or fetch the full set yourself and export it.")]
    public static partial void ExportLimitedToLoadedPage(ILogger logger);
}
