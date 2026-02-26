using System.Linq.Expressions;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Extension methods for applying DataGrid operations (multi-sort, pagination)
/// to both IQueryable and IEnumerable data sources.
/// </summary>
public static class DataGridDataExtensions
{
    /// <summary>
    /// Applies multi-column sorting to an IQueryable data source.
    /// Composes OrderBy/ThenBy LINQ expressions that translate to SQL via EF Core.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="query">The queryable data source.</param>
    /// <param name="sortDefinitions">The sort definitions in priority order.</param>
    /// <param name="columns">The column definitions (used to resolve sort expressions).</param>
    /// <returns>The sorted queryable.</returns>
    public static IQueryable<TData> ApplyMultiSort<TData>(
        this IQueryable<TData> query,
        IReadOnlyList<SortDefinition> sortDefinitions,
        IReadOnlyList<IDataGridColumn<TData>> columns) where TData : class
    {
        if (sortDefinitions.Count == 0)
        {
            return query;
        }

        IOrderedQueryable<TData>? orderedQuery = null;

        foreach (var sortDef in sortDefinitions)
        {
            var column = columns.FirstOrDefault(c => c.ColumnId == sortDef.ColumnId);
            var sortExpression = column?.GetSortExpression();
            if (sortExpression == null)
            {
                continue;
            }

            if (orderedQuery == null)
            {
                orderedQuery = sortDef.Direction == SortDirection.Ascending
                    ? ApplyOrderBy(query, sortExpression)
                    : ApplyOrderByDescending(query, sortExpression);
            }
            else
            {
                orderedQuery = sortDef.Direction == SortDirection.Ascending
                    ? ApplyThenBy(orderedQuery, sortExpression)
                    : ApplyThenByDescending(orderedQuery, sortExpression);
            }
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Applies multi-column sorting to an in-memory collection.
    /// Uses the column's Compare method for type-specific comparison.
    /// </summary>
    /// <typeparam name="TData">The type of data items.</typeparam>
    /// <param name="data">The data to sort.</param>
    /// <param name="sortDefinitions">The sort definitions in priority order.</param>
    /// <param name="columns">The column definitions (used to resolve comparers).</param>
    /// <returns>The sorted data.</returns>
    public static IEnumerable<TData> ApplyMultiSort<TData>(
        this IEnumerable<TData> data,
        IReadOnlyList<SortDefinition> sortDefinitions,
        IReadOnlyList<IDataGridColumn<TData>> columns) where TData : class
    {
        if (sortDefinitions.Count == 0)
        {
            return data;
        }

        // Always create a copy to avoid mutating the original collection in-place
        var list = data.ToList();
        if (list.Count <= 1)
        {
            return list;
        }

        list.Sort((x, y) =>
        {
            foreach (var sortDef in sortDefinitions)
            {
                var column = columns.FirstOrDefault(c => c.ColumnId == sortDef.ColumnId);
                if (column == null)
                {
                    continue;
                }

                var result = column.Compare(x, y);
                if (result != 0)
                {
                    return sortDef.Direction == SortDirection.Ascending ? result : -result;
                }
            }

            return 0;
        });

        return list;
    }

    // Helper methods to call OrderBy/ThenBy with a runtime LambdaExpression
    // These use Expression.Call to build the LINQ method call dynamically

    private static IOrderedQueryable<TData> ApplyOrderBy<TData>(
        IQueryable<TData> query, LambdaExpression keySelector)
    {
        return (IOrderedQueryable<TData>)query.Provider.CreateQuery<TData>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.OrderBy),
                new[] { typeof(TData), keySelector.ReturnType },
                query.Expression,
                Expression.Quote(keySelector)));
    }

    private static IOrderedQueryable<TData> ApplyOrderByDescending<TData>(
        IQueryable<TData> query, LambdaExpression keySelector)
    {
        return (IOrderedQueryable<TData>)query.Provider.CreateQuery<TData>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.OrderByDescending),
                new[] { typeof(TData), keySelector.ReturnType },
                query.Expression,
                Expression.Quote(keySelector)));
    }

    private static IOrderedQueryable<TData> ApplyThenBy<TData>(
        IOrderedQueryable<TData> query, LambdaExpression keySelector)
    {
        return (IOrderedQueryable<TData>)query.Provider.CreateQuery<TData>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.ThenBy),
                new[] { typeof(TData), keySelector.ReturnType },
                query.Expression,
                Expression.Quote(keySelector)));
    }

    private static IOrderedQueryable<TData> ApplyThenByDescending<TData>(
        IOrderedQueryable<TData> query, LambdaExpression keySelector)
    {
        return (IOrderedQueryable<TData>)query.Provider.CreateQuery<TData>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.ThenByDescending),
                new[] { typeof(TData), keySelector.ReturnType },
                query.Expression,
                Expression.Quote(keySelector)));
    }
}
