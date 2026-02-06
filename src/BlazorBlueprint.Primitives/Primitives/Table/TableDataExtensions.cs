namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Extension methods for processing table data (sorting and pagination).
/// </summary>
public static class TableDataExtensions
{
    /// <summary>
    /// Applies sorting to the data based on the sorting state.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="data">The data to sort.</param>
    /// <param name="sorting">The sorting configuration.</param>
    /// <param name="columns">The column definitions.</param>
    /// <returns>The sorted data.</returns>
    public static IEnumerable<TData> ApplySorting<TData>(
        this IEnumerable<TData> data,
        SortingState sorting,
        IEnumerable<IColumnDefinition<TData>> columns) where TData : class
    {
        if (sorting.SortedColumn == null || sorting.Direction == SortDirection.None)
        {
            return data;
        }

        var column = columns.FirstOrDefault(c => c.Id == sorting.SortedColumn);
        if (column == null)
        {
            return data;
        }


        // Optimize: check if already a list to avoid unnecessary allocation
        var dataList = data as IList<TData> ?? data.ToList();

        // For IList, create a sorted copy; for List, sort in place
        if (dataList is List<TData> list)
        {
            // Sort using column's Compare method for type-specific comparison
            list.Sort((x, y) =>
            {
                var result = column.Compare(x, y);
                return sorting.Direction == SortDirection.Ascending ? result : -result;
            });
            return list;
        }
        else
        {
            // For IList (not List<T>), convert and sort
            var sortedList = dataList.ToList();
            sortedList.Sort((x, y) =>
            {
                var result = column.Compare(x, y);
                return sorting.Direction == SortDirection.Ascending ? result : -result;
            });
            return sortedList;
        }
    }

    /// <summary>
    /// Applies pagination to the data based on the pagination state.
    /// Updates the TotalItems property on the pagination state.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="data">The data to paginate.</param>
    /// <param name="pagination">The pagination configuration.</param>
    /// <returns>The paginated data for the current page.</returns>
    public static IEnumerable<TData> ApplyPagination<TData>(
        this IEnumerable<TData> data,
        PaginationState pagination) where TData : class
    {
        // Materialize once to avoid multiple enumeration
        var dataList = data as IList<TData> ?? data.ToList();
        pagination.TotalItems = dataList.Count;

        // Use GetRange for List<T> to avoid iterator overhead, otherwise use Skip/Take
        if (dataList is List<TData> list)
        {
            var startIndex = Math.Min(pagination.StartIndex, list.Count);
            var count = Math.Min(pagination.PageSize, list.Count - startIndex);
            return count > 0 ? list.GetRange(startIndex, count) : Array.Empty<TData>();
        }

        // Fallback for IList<T> - use indexer for better performance than Skip/Take
        var start = Math.Min(pagination.StartIndex, dataList.Count);
        var take = Math.Min(pagination.PageSize, dataList.Count - start);
        var result = new TData[take];
        for (var i = 0; i < take; i++)
        {
            result[i] = dataList[start + i];
        }
        return result;
    }

    /// <summary>
    /// Processes data through the complete pipeline: sorting then pagination.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="data">The raw data.</param>
    /// <param name="state">The table state containing sorting and pagination configuration.</param>
    /// <param name="columns">The column definitions.</param>
    /// <returns>The processed data ready for rendering.</returns>
    public static IEnumerable<TData> ProcessTableData<TData>(
        this IEnumerable<TData> data,
        TableState<TData> state,
        IEnumerable<IColumnDefinition<TData>> columns) where TData : class
    {
        return data
            .ApplySorting(state.Sorting, columns)
            .ApplyPagination(state.Pagination);
    }
}
