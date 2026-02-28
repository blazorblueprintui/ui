namespace BlazorBlueprint.Components;

/// <summary>
/// A heatmap chart component using the declarative composition API with Apache ECharts.
/// </summary>
/// <remarks>
/// <para>
/// The HeatmapChart component provides color-coded grid visualizations where cell color
/// intensity represents the data value. Pair with <see cref="BbVisualMap"/> to configure
/// the color gradient range.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbHeatmapChart Data="@data" Height="350px"&gt;
///     &lt;BbXAxis DataKey="day" /&gt;
///     &lt;BbYAxis DataKey="hour" /&gt;
///     &lt;BbVisualMap Min="0" Max="100" /&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbHeatmap XKey="dayIndex" YKey="hourIndex" ValueKey="value" /&gt;
/// &lt;/BbHeatmapChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbHeatmapChart : BbChartBase
{
    internal override string DataSlot => "heatmap-chart";

    internal override string SeriesType => "heatmap";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
        option.Grid ??= new EChartsGridOption
        {
            Left = "12",
            Right = "12",
            Top = "10",
            Bottom = "50",
            ContainLabel = true
        };
    }
}
