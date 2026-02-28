namespace BlazorBlueprint.Components;

/// <summary>
/// A funnel chart component using the declarative composition API with Apache ECharts.
/// </summary>
/// <remarks>
/// <para>
/// The FunnelChart component provides funnel visualizations for conversion pipelines,
/// sales processes, and progressive filtering. Segments are sized by value and can
/// be sorted ascending, descending, or in data order.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbFunnelChart Data="@data" Height="350px"&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbChartLegend /&gt;
///     &lt;BbFunnel DataKey="value" NameKey="stage" Sort="FunnelSort.Descending" /&gt;
/// &lt;/BbFunnelChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbFunnelChart : BbChartBase
{
    internal override string DataSlot => "funnel-chart";

    internal override string SeriesType => "funnel";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
        // Funnel charts don't use XAxis, YAxis, or Grid
        option.XAxis = null;
        option.YAxis = null;
        option.Grid = null;
    }
}
