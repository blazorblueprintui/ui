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
    public void RegisterWidget(string widgetId, int column, int row, int columnSpan, int rowSpan) =>
        State.RegisterWidget(widgetId, column, row, columnSpan, rowSpan);

    /// <summary>
    /// Removes a widget from all layouts.
    /// </summary>
    public void UnregisterWidget(string widgetId)
    {
        State.UnregisterWidget(widgetId);
        NotifyStateChanged();
    }

    /// <summary>
    /// Gets the current position of a widget from the active layout, falling back to the large layout.
    /// </summary>
    public WidgetPosition? GetWidgetPosition(string widgetId) =>
        State.GetActiveLayout().GetPosition(widgetId)
            ?? State.Large.GetPosition(widgetId);

    /// <summary>
    /// Updates a widget's position and notifies subscribers.
    /// </summary>
    public void UpdateWidgetPosition(string widgetId, int column, int row, int columnSpan, int rowSpan)
    {
        State.UpdateWidgetPosition(widgetId, column, row, columnSpan, rowSpan);
        NotifyStateChanged();
    }

    /// <summary>
    /// Updates a widget's position without notifying.
    /// Use for batching multiple updates, then call <see cref="Notify"/> once at the end.
    /// </summary>
    public void UpdateWidgetPositionSilent(string widgetId, int column, int row, int columnSpan, int rowSpan) =>
        State.UpdateWidgetPosition(widgetId, column, row, columnSpan, rowSpan);

    /// <summary>
    /// Notifies subscribers that the layout state has changed.
    /// </summary>
    public void Notify() => NotifyStateChanged();

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
