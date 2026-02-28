using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Configures the visual map (color gradient) for heatmap and other chart types.
/// Must be placed inside a chart component.
/// </summary>
/// <remarks>
/// <para>
/// VisualMap maps data values to a color gradient, typically used with heatmap charts.
/// It provides an interactive range selector when <see cref="Calculable"/> is true.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbHeatmapChart Data="@data"&gt;
///     &lt;BbVisualMap Min="0" Max="100" /&gt;
///     &lt;BbHeatmap XKey="x" YKey="y" ValueKey="value" /&gt;
/// &lt;/BbHeatmapChart&gt;
/// </code>
/// </example>
public partial class BbVisualMap : ComponentBase, IChartComponent, IDisposable
{
    [CascadingParameter]
    private BbChartBase? ParentChart { get; set; }

    /// <summary>
    /// Gets or sets the minimum value of the visual map range.
    /// </summary>
    [Parameter]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the visual map range.
    /// </summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether the visual map is visible.
    /// </summary>
    [Parameter]
    public bool Show { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the range handles are draggable.
    /// </summary>
    [Parameter]
    public bool Calculable { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the visual map control.
    /// </summary>
    /// <remarks>
    /// Accepts <c>"horizontal"</c> or <c>"vertical"</c>. Default is <c>"horizontal"</c>.
    /// </remarks>
    [Parameter]
    public string Orient { get; set; } = "horizontal";

    /// <summary>
    /// Gets or sets the color gradient for the visual map range.
    /// </summary>
    /// <remarks>
    /// Colors are mapped from <see cref="Min"/> to <see cref="Max"/>. When null, defaults to
    /// <c>["var(--chart-5)", "var(--chart-1)"]</c>.
    /// </remarks>
    [Parameter]
    public string[]? Colors { get; set; }

    protected override void OnInitialized() =>
        ParentChart?.RegisterComponent(this);

    void IChartComponent.ApplyTo(EChartsOption option)
    {
        var colors = Colors ?? ["var(--chart-5)", "var(--chart-1)"];

        option.VisualMap = new EChartsVisualMapOption
        {
            Min = Min,
            Max = Max,
            Show = Show,
            Calculable = Calculable,
            Orient = Orient,
            Left = "center",
            Bottom = "0",
            Type = "continuous",
            InRange = new EChartsVisualMapInRangeOption
            {
                Color = [.. colors]
            }
        };
    }

    public void Dispose()
    {
        ParentChart?.UnregisterComponent(this);
        GC.SuppressFinalize(this);
    }
}
