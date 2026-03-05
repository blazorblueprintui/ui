namespace BlazorBlueprint.Primitives.DashboardGrid;

/// <summary>
/// Collection of widget positions for a single breakpoint.
/// </summary>
public class DashboardGridLayout
{
    public List<WidgetPosition> Positions { get; set; } = new();

    public WidgetPosition? GetPosition(string widgetId) =>
        Positions.Find(p => p.WidgetId == widgetId);

    public void SetPosition(string widgetId, int column, int row, int columnSpan, int rowSpan)
    {
        var existing = GetPosition(widgetId);
        if (existing != null)
        {
            existing.Column = column;
            existing.Row = row;
            existing.ColumnSpan = columnSpan;
            existing.RowSpan = rowSpan;
        }
        else
        {
            Positions.Add(new WidgetPosition(widgetId, column, row, columnSpan, rowSpan));
        }
    }

    public void RemoveWidget(string widgetId) =>
        Positions.RemoveAll(p => p.WidgetId == widgetId);

    /// <summary>
    /// Compacts the layout by sliding widgets toward the top-left to fill gaps.
    /// Preserves relative widget ordering. Widgets can be pushed right/down when
    /// a neighbor expands, but prefer leftward/upward positions to close gaps.
    /// </summary>
    public void Compact(int columns, string? fixedWidgetId = null)
    {
        var sorted = Positions.OrderBy(p => p.Row).ThenBy(p => p.Column).ToList();
        List<WidgetPosition> placed = [];

        // Pre-place fixed widget so all others compact around it
        if (fixedWidgetId != null)
        {
            var fixedWidget = sorted.Find(p => p.WidgetId == fixedWidgetId);
            if (fixedWidget != null)
            {
                placed.Add(fixedWidget);
            }
        }

        foreach (var widget in sorted)
        {
            if (fixedWidgetId != null && widget.WidgetId == fixedWidgetId)
            {
                continue;
            }

            var found = false;

            // Try current row first — slide left to close horizontal gaps
            for (var col = 1; col <= columns - widget.ColumnSpan + 1; col++)
            {
                if (!OverlapsAny(col, widget.Row, widget.ColumnSpan, widget.RowSpan, placed))
                {
                    widget.Column = col;
                    found = true;
                    break;
                }
            }

            // If pushed off current row by an expanded widget, find the next row that fits
            for (var row = widget.Row + 1; !found && row <= widget.Row + 100; row++)
            {
                for (var col = 1; col <= columns - widget.ColumnSpan + 1; col++)
                {
                    if (!OverlapsAny(col, row, widget.ColumnSpan, widget.RowSpan, placed))
                    {
                        widget.Row = row;
                        widget.Column = col;
                        found = true;
                        break;
                    }
                }
            }

            placed.Add(widget);
        }

        // Second pass: close vertical gaps by sliding widgets up
        // without changing their column or relative row ordering
        placed.Clear();
        sorted = Positions.OrderBy(p => p.Row).ThenBy(p => p.Column).ToList();

        // Pre-place fixed widget again for second pass
        if (fixedWidgetId != null)
        {
            var fixedWidget = sorted.Find(p => p.WidgetId == fixedWidgetId);
            if (fixedWidget != null)
            {
                placed.Add(fixedWidget);
            }
        }

        foreach (var widget in sorted)
        {
            if (fixedWidgetId != null && widget.WidgetId == fixedWidgetId)
            {
                continue;
            }

            // Find the topmost row this widget can occupy
            for (var row = 1; row <= widget.Row; row++)
            {
                if (!OverlapsAny(widget.Column, row, widget.ColumnSpan, widget.RowSpan, placed))
                {
                    widget.Row = row;
                    break;
                }
            }

            placed.Add(widget);
        }
    }

    /// <summary>
    /// Closes vertical gaps by sliding widgets upward without changing columns.
    /// Use after JS-resolved layouts where horizontal positions are already correct.
    /// </summary>
    public void CompactVertical(string? fixedWidgetId = null)
    {
        var sorted = Positions.OrderBy(p => p.Row).ThenBy(p => p.Column).ToList();
        List<WidgetPosition> placed = [];

        if (fixedWidgetId != null)
        {
            var fixedWidget = sorted.Find(p => p.WidgetId == fixedWidgetId);
            if (fixedWidget != null)
            {
                placed.Add(fixedWidget);
            }
        }

        foreach (var widget in sorted)
        {
            if (fixedWidgetId != null && widget.WidgetId == fixedWidgetId)
            {
                continue;
            }

            for (var row = 1; row <= widget.Row; row++)
            {
                if (!OverlapsAny(widget.Column, row, widget.ColumnSpan, widget.RowSpan, placed))
                {
                    widget.Row = row;
                    break;
                }
            }

            placed.Add(widget);
        }
    }

    private static bool OverlapsAny(int col, int row, int colSpan, int rowSpan, List<WidgetPosition> others)
    {
        foreach (var o in others)
        {
            if (col < o.Column + o.ColumnSpan &&
                o.Column < col + colSpan &&
                row < o.Row + o.RowSpan &&
                o.Row < row + rowSpan)
            {
                return true;
            }
        }
        return false;
    }

    public DashboardGridLayout Clone()
    {
        var clone = new DashboardGridLayout();
        foreach (var pos in Positions)
        {
            clone.Positions.Add(pos.Clone());
        }
        return clone;
    }
}
