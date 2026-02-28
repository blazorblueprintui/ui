using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A scatter/bubble series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a scatter series within a parent chart. Supports configurable symbol sizes
/// including per-item sizing for bubble charts via <see cref="SymbolSizeKey"/>.
/// </para>
/// <para>
/// Must be placed inside a chart component that provides a <see cref="BbChartBase"/> cascading value.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbScatterChart Data="@data"&gt;
///     &lt;BbXAxis DataKey="x" Type="AxisType.Value" /&gt;
///     &lt;BbYAxis /&gt;
///     &lt;BbScatter DataKey="y" Name="Series A" SymbolSize="12" /&gt;
/// &lt;/BbScatterChart&gt;
/// </code>
/// </example>
public partial class BbScatter : SeriesBase
{
    /// <summary>
    /// Gets or sets the size of scatter symbols in pixels.
    /// </summary>
    [Parameter]
    public int SymbolSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the property name used to extract per-item symbol sizes from the chart data.
    /// </summary>
    /// <remarks>
    /// When set, each data point's symbol size is read from this property, enabling
    /// bubble chart visualizations where dot size represents a third dimension.
    /// </remarks>
    [Parameter]
    public string? SymbolSizeKey { get; set; }

    /// <summary>
    /// Gets or sets whether data labels are displayed on each point.
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; }

    /// <summary>
    /// Gets or sets the position of the data label relative to the point.
    /// </summary>
    [Parameter]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Top;

    /// <summary>
    /// Gets or sets the label formatter string.
    /// </summary>
    /// <remarks>
    /// ECharts formatter string. Use <c>{b}</c> for name, <c>{c}</c> for value.
    /// </remarks>
    [Parameter]
    public string? LabelFormatter { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();
        var effectiveColor = GetResolvedFillColor() ?? resolvedColor;
        var rawData = GetSeriesData();

        var series = new EChartsSeriesOption
        {
            Type = "scatter",
            Name = GetResolvedName(),
            SymbolSize = SymbolSize,
            Emphasis = new EChartsEmphasisOption
            {
                Focus = "series"
            }
        };

        if (!string.IsNullOrEmpty(SymbolSizeKey))
        {
            var sizes = DataExtractor.ExtractValues(ParentChart?.Data, SymbolSizeKey);
            var itemData = new List<object?>(rawData.Count);

            for (var i = 0; i < rawData.Count; i++)
            {
                var size = (i < sizes.Count) ? sizes[i] : null;
                itemData.Add(new Dictionary<string, object?>
                {
                    ["value"] = rawData[i],
                    ["symbolSize"] = size
                });
            }

            series.Data = itemData;
            series.SymbolSize = null;
        }
        else
        {
            series.Data = rawData;
        }

        if (effectiveColor != null)
        {
            series.ItemStyle = new EChartsItemStyleOption
            {
                Color = effectiveColor
            };
        }

        if (ShowLabel || !string.IsNullOrEmpty(LabelFormatter))
        {
            series.Label = new EChartsLabelOption
            {
                Show = true,
                Position = ToEChartsPosition(LabelPosition),
                Formatter = LabelFormatter
            };
        }

        if (Stacked)
        {
            series.Stack = StackGroup;
        }

        return series;
    }
}
