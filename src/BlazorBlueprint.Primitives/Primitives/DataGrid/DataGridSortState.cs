namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Manages multi-column sorting state for the DataGrid.
/// Supports adding, toggling, and removing sort definitions across multiple columns.
/// </summary>
public class DataGridSortState
{
    private readonly List<SortDefinition> definitions = new();

    /// <summary>
    /// Gets the current sort definitions in priority order.
    /// The first definition has the highest sort priority.
    /// </summary>
    public IReadOnlyList<SortDefinition> Definitions => definitions;

    /// <summary>
    /// Gets whether any sorting is applied.
    /// </summary>
    public bool HasSorting => definitions.Count > 0;

    /// <summary>
    /// Toggles sorting for a column with three-state behavior: None -> Ascending -> Descending -> None.
    /// When <paramref name="multiSort"/> is false, replaces all existing sort definitions.
    /// When <paramref name="multiSort"/> is true, adds to or modifies existing definitions.
    /// </summary>
    /// <param name="columnId">The ID of the column to toggle.</param>
    /// <param name="multiSort">Whether to support multi-column sorting (e.g., Ctrl+Click).</param>
    public void ToggleSort(string columnId, bool multiSort = false)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            throw new ArgumentException("Column ID cannot be null or whitespace.", nameof(columnId));
        }

        var existing = definitions.FirstOrDefault(d => d.ColumnId == columnId);

        if (existing != null)
        {
            // When not multi-sorting, clear all other columns first (switch to single-sort)
            if (!multiSort)
            {
                definitions.RemoveAll(d => d.ColumnId != columnId);
            }

            // Cycle through states: Ascending -> Descending -> Remove
            if (existing.Direction == SortDirection.Ascending)
            {
                existing.Direction = SortDirection.Descending;
            }
            else
            {
                definitions.Remove(existing);
            }
        }
        else
        {
            // New column — if not multi-sort, clear existing definitions first
            if (!multiSort)
            {
                definitions.Clear();
            }

            definitions.Add(new SortDefinition
            {
                ColumnId = columnId,
                Direction = SortDirection.Ascending
            });
        }
    }

    /// <summary>
    /// Sets a single sort definition, replacing all existing definitions.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    /// <param name="direction">The sort direction.</param>
    public void SetSort(string columnId, SortDirection direction)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            throw new ArgumentException("Column ID cannot be null or whitespace.", nameof(columnId));
        }

        definitions.Clear();

        if (direction != SortDirection.None)
        {
            definitions.Add(new SortDefinition
            {
                ColumnId = columnId,
                Direction = direction
            });
        }
    }

    /// <summary>
    /// Clears all sort definitions.
    /// </summary>
    public void ClearSort() => definitions.Clear();

    /// <summary>
    /// Gets the sort direction for a specific column.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>The sort direction, or <see cref="SortDirection.None"/> if the column is not sorted.</returns>
    public SortDirection GetDirection(string columnId)
    {
        var definition = definitions.FirstOrDefault(d => d.ColumnId == columnId);
        return definition?.Direction ?? SortDirection.None;
    }

    /// <summary>
    /// Gets the sort priority for a specific column (1-based).
    /// Returns 0 if the column is not in the sort definitions.
    /// Used for multi-sort priority indicators in the UI.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>The 1-based priority, or 0 if not sorted.</returns>
    public int GetPriority(string columnId)
    {
        for (var i = 0; i < definitions.Count; i++)
        {
            if (definitions[i].ColumnId == columnId)
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets whether a specific column is currently sorted.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>True if the column is being sorted.</returns>
    public bool IsSorted(string columnId) => definitions.Any(d => d.ColumnId == columnId);
}
