using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A radar series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a radar (spider/web) series within a parent chart. Data values are plotted
/// as a polygon on the radar axes. Supports configurable fill opacity, dot visibility,
/// and stroke width.
/// </para>
/// <para>
/// Must be placed inside a chart component that provides a <see cref="BbChartBase"/> cascading value
/// with a radar coordinate system configured.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;RadarChart Data="@data"&gt;
///     &lt;Radar DataKey="desktop" Name="Desktop" FillOpacity="0.3" /&gt;
///     &lt;Radar DataKey="mobile" Name="Mobile" FillOpacity="0.3" /&gt;
/// &lt;/RadarChart&gt;
/// </code>
/// </example>
public partial class BbRadar : SeriesBase
{
    /// <summary>
    /// Gets or sets the opacity of the filled area within the radar polygon.
    /// </summary>
    /// <remarks>
    /// Value between 0 (transparent) and 1 (opaque). Default is 0.25.
    /// Set to 0 to show only the outline.
    /// </remarks>
    [Parameter]
    public double FillOpacity { get; set; } = 0.25;

    /// <summary>
    /// Gets or sets whether data point dots are visible on the radar vertices.
    /// </summary>
    [Parameter]
    public bool ShowDots { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of data point dots in pixels.
    /// </summary>
    [Parameter]
    public int DotSize { get; set; } = 4;

    /// <summary>
    /// Gets or sets the line stroke width in pixels.
    /// </summary>
    [Parameter]
    public int StrokeWidth { get; set; } = 2;

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();
        var data = GetSeriesData();

        var series = new EChartsSeriesOption
        {
            Type = "radar",
            Name = GetResolvedName(),
            SymbolSize = ShowDots ? DotSize : 0,
            LineStyle = new EChartsLineStyleOption
            {
                Width = StrokeWidth,
                Color = resolvedColor
            },
            ItemStyle = resolvedColor != null
                ? new EChartsItemStyleOption { Color = resolvedColor }
                : null,
            Emphasis = new EChartsEmphasisOption { Disabled = true }
        };

        if (FillOpacity > 0)
        {
            series.AreaStyle = new EChartsAreaStyleOption
            {
                Opacity = FillOpacity,
                Color = FillColor
            };
        }

        // Radar series data is structured as [{value: [...], name: "..."}]
        series.Data = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["value"] = data,
                ["name"] = GetResolvedName()
            }
        };

        return series;
    }
}
