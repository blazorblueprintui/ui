namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Tracks column visibility, order, and width state for the DataGrid.
/// </summary>
public class DataGridColumnState
{
    private readonly List<ColumnStateEntry> entries = new();

    /// <summary>
    /// Gets the column state entries.
    /// </summary>
    public IReadOnlyList<ColumnStateEntry> Entries => entries;

    /// <summary>
    /// Initializes column state from a list of column definitions.
    /// Preserves existing entries and adds new ones for unknown columns.
    /// New entries use the provided initial visibility; existing entries retain their current visibility.
    /// </summary>
    /// <param name="columns">The column IDs and their initial visibility.</param>
    public void Initialize(IEnumerable<(string ColumnId, bool Visible)> columns)
    {
        var activeIds = new HashSet<string>();
        var order = 0;
        foreach (var (id, visible) in columns)
        {
            activeIds.Add(id);
            var existing = entries.FirstOrDefault(e => e.ColumnId == id);
            if (existing == null)
            {
                entries.Add(new ColumnStateEntry
                {
                    ColumnId = id,
                    Visible = visible,
                    Order = order
                });
            }
            else
            {
                existing.Order = order;
            }

            order++;
        }

        // Remove entries for columns that no longer exist (e.g., from stale persisted state)
        entries.RemoveAll(e => !activeIds.Contains(e.ColumnId));

        NormalizeOrders();
    }

    /// <summary>
    /// Adds entries for columns that are not tracked yet, leaving every existing entry's
    /// visibility, width, and relative order untouched.
    /// </summary>
    /// <remarks>
    /// Use this instead of <see cref="Initialize"/> once the state has been initialized, so a
    /// column that registers late — from a wrapper component or an asynchronously rendered
    /// fragment — still gets an entry and stays visible. Each new entry is placed immediately
    /// after the nearest preceding tracked column in <paramref name="columns"/>, so the new
    /// column lands in its intended position rather than being appended to the end.
    /// Unlike <see cref="Initialize"/>, this does not renumber existing entries and does not
    /// remove entries for columns missing from <paramref name="columns"/>, so a user's column
    /// reordering and visibility choices survive.
    /// </remarks>
    /// <param name="columns">The column IDs in display order, and their initial visibility.</param>
    /// <returns>True if any entry was added.</returns>
    public bool SyncColumns(IEnumerable<(string ColumnId, bool Visible)> columns)
    {
        var tracked = new Dictionary<string, ColumnStateEntry>(entries.Count, StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            tracked[entry.ColumnId] = entry;
        }

        var added = false;
        ColumnStateEntry? previous = null;

        foreach (var (id, visible) in columns)
        {
            if (tracked.TryGetValue(id, out var existing))
            {
                previous = existing;
                continue;
            }

            var insertOrder = previous != null ? previous.Order + 1 : 0;
            foreach (var entry in entries)
            {
                if (entry.Order >= insertOrder)
                {
                    entry.Order++;
                }
            }

            var created = new ColumnStateEntry
            {
                ColumnId = id,
                Visible = visible,
                Order = insertOrder
            };

            entries.Add(created);
            tracked[id] = created;
            previous = created;
            added = true;
        }

        if (added)
        {
            NormalizeOrders();
        }

        return added;
    }

    /// <summary>
    /// Sets the visibility of a column.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <param name="visible">Whether the column should be visible.</param>
    public void SetVisibility(string columnId, bool visible)
    {
        var entry = GetOrCreateEntry(columnId);
        entry.Visible = visible;
    }

    /// <summary>
    /// Sets the width of a column.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <param name="width">The width value (e.g., "200px", "20%").</param>
    public void SetWidth(string columnId, string? width)
    {
        var entry = GetOrCreateEntry(columnId);
        entry.Width = width;
    }

    /// <summary>
    /// Moves a column to a new position in the order.
    /// </summary>
    /// <param name="columnId">The column ID to move.</param>
    /// <param name="newIndex">The new zero-based position.</param>
    public void ReorderColumn(string columnId, int newIndex)
    {
        var entry = entries.FirstOrDefault(e => e.ColumnId == columnId);
        if (entry == null)
        {
            return;
        }

        // Clamp newIndex to a valid range
        var maxIndex = entries.Count - 1;
        newIndex = Math.Clamp(newIndex, 0, maxIndex);

        var currentIndex = entry.Order;
        if (currentIndex == newIndex)
        {
            return;
        }

        // Shift other entries to accommodate the move
        foreach (var e in entries)
        {
            if (e.ColumnId == columnId)
            {
                continue;
            }

            if (currentIndex < newIndex)
            {
                // Moving right: shift entries between old and new position left
                if (e.Order > currentIndex && e.Order <= newIndex)
                {
                    e.Order--;
                }
            }
            else
            {
                // Moving left: shift entries between new and old position right
                if (e.Order >= newIndex && e.Order < currentIndex)
                {
                    e.Order++;
                }
            }
        }

        entry.Order = newIndex;

        // Normalize orders to be contiguous (0..N-1) to prevent gaps
        NormalizeOrders();
    }

    /// <summary>
    /// Gets column IDs ordered by their current position, filtered to visible columns only.
    /// </summary>
    /// <returns>Ordered list of visible column IDs.</returns>
    public IReadOnlyList<string> GetVisibleColumnIds()
    {
        return entries
            .Where(e => e.Visible)
            .OrderBy(e => e.Order)
            .Select(e => e.ColumnId)
            .ToList();
    }

    /// <summary>
    /// Gets the visibility state of a column.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>True if visible, or true by default if not tracked.</returns>
    public bool IsVisible(string columnId)
    {
        var entry = entries.FirstOrDefault(e => e.ColumnId == columnId);
        return entry?.Visible ?? true;
    }

    /// <summary>
    /// Gets the width of a column.
    /// </summary>
    /// <param name="columnId">The column ID to check.</param>
    /// <returns>The width, or null if not set.</returns>
    public string? GetWidth(string columnId)
    {
        var entry = entries.FirstOrDefault(e => e.ColumnId == columnId);
        return entry?.Width;
    }

    /// <summary>
    /// Resets all column state (visibility, widths, order).
    /// </summary>
    public void Reset() => entries.Clear();

    /// <summary>
    /// Replaces all entries from a collection of snapshots.
    /// Used internally for restoring state from a persisted snapshot.
    /// </summary>
    /// <param name="snapshots">The column state snapshots to restore.</param>
    internal void RestoreFromSnapshots(IEnumerable<ColumnStateSnapshot> snapshots)
    {
        entries.Clear();
        foreach (var snapshot in snapshots)
        {
            entries.Add(new ColumnStateEntry
            {
                ColumnId = snapshot.ColumnId,
                Visible = snapshot.Visible,
                Width = snapshot.Width,
                Order = snapshot.Order
            });
        }
    }

    private void NormalizeOrders()
    {
        var sorted = entries.OrderBy(e => e.Order).ToList();
        for (var i = 0; i < sorted.Count; i++)
        {
            sorted[i].Order = i;
        }
    }

    private ColumnStateEntry GetOrCreateEntry(string columnId)
    {
        var entry = entries.FirstOrDefault(e => e.ColumnId == columnId);
        if (entry == null)
        {
            entry = new ColumnStateEntry
            {
                ColumnId = columnId,
                Order = entries.Count
            };
            entries.Add(entry);
        }

        return entry;
    }
}

/// <summary>
/// Represents the state of a single column in the DataGrid.
/// </summary>
public class ColumnStateEntry
{
    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public required string ColumnId { get; set; }

    /// <summary>
    /// Gets or sets whether the column is visible. Default is true.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the column width (e.g., "200px", "20%"). Null for auto.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the display order of the column (zero-based).
    /// </summary>
    public int Order { get; set; }
}
