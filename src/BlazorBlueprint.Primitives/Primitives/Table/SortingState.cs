namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Manages the sorting configuration for a table.
/// Supports single-column sorting with three-state toggle (none → ascending → descending → none).
/// </summary>
public class SortingState
{
    /// <summary>
    /// Gets or sets the ID of the currently sorted column.
    /// Null when no sorting is applied.
    /// </summary>
    public string? SortedColumn { get; set; }

    /// <summary>
    /// Gets or sets the current sort direction.
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.None;

    /// <summary>
    /// Sets the sort state for a specific column.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    /// <param name="direction">The sort direction.</param>
    /// <exception cref="ArgumentException">Thrown when columnId is null or whitespace.</exception>
    public void SetSort(string columnId, SortDirection direction)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            throw new ArgumentException("Column ID cannot be null or whitespace.", nameof(columnId));
        }


        SortedColumn = columnId;
        Direction = direction;
    }

    /// <summary>
    /// Toggles the sort direction for a column with three-state behavior:
    /// None → Ascending → Descending → None.
    /// If a different column is clicked, it starts with Ascending.
    /// </summary>
    /// <param name="columnId">The ID of the column to toggle.</param>
    /// <exception cref="ArgumentException">Thrown when columnId is null or whitespace.</exception>
    /// <remarks>
    /// When cycling back to None, SortedColumn is set to null to clearly indicate no sorting is active.
    /// This provides a clean state for the "no sort" condition and simplifies state checks.
    /// </remarks>
    public void ToggleSort(string columnId)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            throw new ArgumentException("Column ID cannot be null or whitespace.", nameof(columnId));
        }


        if (SortedColumn == columnId)
        {
            // Same column - cycle through states
            Direction = Direction switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.None
            };

            // Clear column when returning to None for clean state management
            if (Direction == SortDirection.None)
            {
                SortedColumn = null;
            }

        }
        else
        {
            // Different column - start with Ascending
            SortedColumn = columnId;
            Direction = SortDirection.Ascending;
        }
    }

    /// <summary>
    /// Clears all sorting state.
    /// </summary>
    public void ClearSort()
    {
        SortedColumn = null;
        Direction = SortDirection.None;
    }

    /// <summary>
    /// Checks if a specific column is currently being sorted.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>True if the column is being sorted, false otherwise.</returns>
    public bool IsSorted(string columnId) => SortedColumn == columnId && Direction != SortDirection.None;

    /// <summary>
    /// Gets the sort direction for a specific column.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>The sort direction, or None if the column is not being sorted.</returns>
    public SortDirection GetDirection(string columnId) => SortedColumn == columnId ? Direction : SortDirection.None;
}
