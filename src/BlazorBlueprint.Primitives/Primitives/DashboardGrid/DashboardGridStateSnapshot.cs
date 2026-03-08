namespace BlazorBlueprint.Primitives.DashboardGrid;

/// <summary>
/// Serializable snapshot of dashboard grid state for persistence.
/// </summary>
public class DashboardGridStateSnapshot
{
    public List<WidgetPositionSnapshot> LargeLayout { get; set; } = new();
    public List<WidgetPositionSnapshot>? MediumLayout { get; set; }
    public List<WidgetPositionSnapshot>? SmallLayout { get; set; }
}

/// <summary>
/// Serializable snapshot of a single widget position.
/// </summary>
public class WidgetPositionSnapshot
{
    public string WidgetId { get; set; } = string.Empty;
    public int Column { get; set; }
    public int Row { get; set; }
    public int ColumnSpan { get; set; }
    public int RowSpan { get; set; }
}
