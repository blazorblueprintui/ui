namespace BlazorBlueprint.Components;

/// <summary>
/// A scatter chart component using the declarative composition API with Apache ECharts.
/// </summary>
/// <remarks>
/// <para>
/// The ScatterChart component provides scatter plot and bubble chart visualizations.
/// Use <see cref="BbScatter.SymbolSizeKey"/> on the series for bubble charts where
/// dot size represents a data dimension.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbScatterChart Data="@data" Height="350px"&gt;
///     &lt;BbXAxis DataKey="x" /&gt;
///     &lt;BbYAxis /&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbScatter DataKey="y" Name="Series A" Color="var(--chart-1)" /&gt;
/// &lt;/BbScatterChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbScatterChart : BbChartBase
{
    internal override string DataSlot => "scatter-chart";

    internal override string SeriesType => "scatter";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
        option.Grid ??= new EChartsGridOption
        {
            Left = "12",
            Right = "12",
            Top = "10",
            Bottom = "0",
            ContainLabel = true
        };

        option.YAxis ??= new EChartsAxisOption
        {
            Type = "value",
            SplitLine = new EChartsSplitLineOption
            {
                Show = true,
                LineStyle = new EChartsLineStyleOption
                {
                    Color = "var(--border)",
                    Type = "dashed"
                }
            },
            AxisLabel = new EChartsAxisLabelOption
            {
                Color = "var(--muted-foreground)"
            }
        };
    }
}
