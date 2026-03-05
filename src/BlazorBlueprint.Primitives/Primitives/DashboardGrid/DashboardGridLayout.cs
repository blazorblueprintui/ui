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

    public DashboardGridLayout Clone()
    {
        var clone = new DashboardGridLayout();
        foreach (var pos in Positions)
        {
            clone.Positions.Add(pos with { });
        }
        return clone;
    }
}
