namespace BlazorBlueprint.Primitives.DashboardGrid;

/// <summary>
/// Aggregate state container for the DashboardGrid component.
/// Manages layouts per breakpoint with save/restore support.
/// </summary>
public class DashboardGridState
{
    public DashboardGridLayout Large { get; set; } = new();
    public DashboardGridLayout? Medium { get; set; }
    public DashboardGridLayout? Small { get; set; }
    public DashboardBreakpoint ActiveBreakpoint { get; set; } = DashboardBreakpoint.Large;
    public int Version { get; private set; }

    /// <summary>
    /// Gets the active layout with fallback chain: Small → Medium → Large.
    /// </summary>
    public DashboardGridLayout GetActiveLayout()
    {
        return ActiveBreakpoint switch
        {
            DashboardBreakpoint.Small => Small ?? Medium ?? Large,
            DashboardBreakpoint.Medium => Medium ?? Large,
            _ => Large
        };
    }

    /// <summary>
    /// Updates a widget position in the active layout.
    /// </summary>
    public void UpdateWidgetPosition(string widgetId, int column, int row, int columnSpan, int rowSpan) =>
        GetActiveLayout().SetPosition(widgetId, column, row, columnSpan, rowSpan);

    /// <summary>
    /// Registers a widget with its initial position if not already present.
    /// </summary>
    public void RegisterWidget(string widgetId, int column, int row, int columnSpan, int rowSpan)
    {
        if (Large.GetPosition(widgetId) == null)
        {
            Large.SetPosition(widgetId, column, row, columnSpan, rowSpan);
            Version++;
        }
    }

    /// <summary>
    /// Removes a widget from all layouts.
    /// </summary>
    public void UnregisterWidget(string widgetId)
    {
        Large.RemoveWidget(widgetId);
        Medium?.RemoveWidget(widgetId);
        Small?.RemoveWidget(widgetId);
    }

    /// <summary>
    /// Creates a serializable snapshot of all layouts.
    /// </summary>
    public DashboardGridStateSnapshot Save()
    {
        var snapshot = new DashboardGridStateSnapshot();

        foreach (var pos in Large.Positions)
        {
            snapshot.LargeLayout.Add(ToSnapshot(pos));
        }

        if (Medium != null)
        {
            snapshot.MediumLayout = new List<WidgetPositionSnapshot>();
            foreach (var pos in Medium.Positions)
            {
                snapshot.MediumLayout.Add(ToSnapshot(pos));
            }
        }

        if (Small != null)
        {
            snapshot.SmallLayout = new List<WidgetPositionSnapshot>();
            foreach (var pos in Small.Positions)
            {
                snapshot.SmallLayout.Add(ToSnapshot(pos));
            }
        }

        return snapshot;
    }

    /// <summary>
    /// Restores state from a snapshot.
    /// </summary>
    public void Restore(DashboardGridStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        Large = new DashboardGridLayout();
        foreach (var s in snapshot.LargeLayout)
        {
            Large.SetPosition(s.WidgetId, s.Column, s.Row, s.ColumnSpan, s.RowSpan);
        }

        if (snapshot.MediumLayout != null)
        {
            Medium = new DashboardGridLayout();
            foreach (var s in snapshot.MediumLayout)
            {
                Medium.SetPosition(s.WidgetId, s.Column, s.Row, s.ColumnSpan, s.RowSpan);
            }
        }
        else
        {
            Medium = null;
        }

        if (snapshot.SmallLayout != null)
        {
            Small = new DashboardGridLayout();
            foreach (var s in snapshot.SmallLayout)
            {
                Small.SetPosition(s.WidgetId, s.Column, s.Row, s.ColumnSpan, s.RowSpan);
            }
        }
        else
        {
            Small = null;
        }

        Version++;
    }

    /// <summary>
    /// Resets all layouts to empty.
    /// </summary>
    public void Reset()
    {
        Large = new DashboardGridLayout();
        Medium = null;
        Small = null;
        Version++;
    }

    private static WidgetPositionSnapshot ToSnapshot(WidgetPosition pos) =>
        new()
        {
            WidgetId = pos.WidgetId,
            Column = pos.Column,
            Row = pos.Row,
            ColumnSpan = pos.ColumnSpan,
            RowSpan = pos.RowSpan
        };

    private static WidgetPosition FromSnapshot(WidgetPositionSnapshot s) =>
        new(s.WidgetId, s.Column, s.Row, s.ColumnSpan, s.RowSpan);
}
