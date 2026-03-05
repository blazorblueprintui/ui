using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.DashboardGrid;

/// <summary>
/// Context for DashboardGrid primitive and its children.
/// Manages widget registration and position resolution.
/// </summary>
public class DashboardGridContext : PrimitiveContextWithEvents<DashboardGridState>
{
    public int Columns { get; set; } = 12;
    public int RowHeight { get; set; } = 80;
    public int Gap { get; set; } = 16;
    public bool AllowDrag { get; set; } = true;
    public bool AllowResize { get; set; } = true;
    public bool Editable { get; set; } = true;
    public bool Compact { get; set; } = true;

    public DashboardGridContext() : base(new DashboardGridState(), "dashboard-grid")
    {
    }

    public DashboardGridContext(DashboardGridState state) : base(state, "dashboard-grid")
    {
    }

    /// <summary>
    /// Registers a widget and sets its initial position if not already present.
    /// </summary>
    public void RegisterWidget(string widgetId, int column, int row, int columnSpan, int rowSpan)
    {
        State.RegisterWidget(widgetId, column, row, columnSpan, rowSpan);
        if (Compact)
        {
            State.GetActiveLayout().Compact(Columns);
        }
    }

    /// <summary>
    /// Removes a widget from all layouts.
    /// </summary>
    public void UnregisterWidget(string widgetId)
    {
        State.UnregisterWidget(widgetId);
        if (Compact)
        {
            State.GetActiveLayout().Compact(Columns);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Gets the current position of a widget from the active layout, falling back to the large layout.
    /// </summary>
    public WidgetPosition? GetWidgetPosition(string widgetId) =>
        State.GetActiveLayout().GetPosition(widgetId)
            ?? State.Large.GetPosition(widgetId);

    /// <summary>
    /// Updates a widget's position after drag or resize.
    /// </summary>
    public void UpdateWidgetPosition(string widgetId, int column, int row, int columnSpan, int rowSpan)
    {
        State.UpdateWidgetPosition(widgetId, column, row, columnSpan, rowSpan);
        if (Compact)
        {
            State.GetActiveLayout().Compact(Columns);
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Updates a widget's position without compacting or notifying.
    /// Use for batching multiple updates, then call <see cref="CompactAndNotify"/> once at the end.
    /// </summary>
    public void UpdateWidgetPositionSilent(string widgetId, int column, int row, int columnSpan, int rowSpan) =>
        State.UpdateWidgetPosition(widgetId, column, row, columnSpan, rowSpan);

    /// <summary>
    /// Compacts the active layout and notifies subscribers. Call after batching silent updates.
    /// </summary>
    public void CompactAndNotify(string? fixedWidgetId = null)
    {
        if (Compact)
        {
            State.GetActiveLayout().CompactVertical(fixedWidgetId);
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the active breakpoint.
    /// </summary>
    public void SetActiveBreakpoint(DashboardBreakpoint breakpoint)
    {
        if (State.ActiveBreakpoint != breakpoint)
        {
            State.ActiveBreakpoint = breakpoint;
            NotifyStateChanged();
        }
    }
}
