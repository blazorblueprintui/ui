using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A heatmap series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a heatmap series within a parent chart. Data is extracted as X-Y-value
/// triplets using <see cref="XKey"/>, <see cref="YKey"/>, and <see cref="ValueKey"/>.
/// Pair with a <see cref="BbVisualMap"/> composable to configure the color gradient.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbHeatmapChart Data="@data"&gt;
///     &lt;BbXAxis DataKey="day" /&gt;
///     &lt;BbYAxis DataKey="hour" /&gt;
///     &lt;BbVisualMap Min="0" Max="100" /&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbHeatmap XKey="dayIndex" YKey="hourIndex" ValueKey="value" /&gt;
/// &lt;/BbHeatmapChart&gt;
/// </code>
/// </example>
public partial class BbHeatmap : SeriesBase
{
    /// <summary>
    /// Gets or sets the property name for the X coordinate (column index or category index).
    /// </summary>
    [Parameter]
    public string? XKey { get; set; }

    /// <summary>
    /// Gets or sets the property name for the Y coordinate (row index or category index).
    /// </summary>
    [Parameter]
    public string? YKey { get; set; }

    /// <summary>
    /// Gets or sets the property name for the cell value.
    /// </summary>
    [Parameter]
    public string? ValueKey { get; set; }

    /// <summary>
    /// Gets or sets whether data labels are displayed in each cell.
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; }

    /// <summary>
    /// Gets or sets the label formatter string.
    /// </summary>
    [Parameter]
    public string? LabelFormatter { get; set; }

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    [Parameter]
    public string? LabelColor { get; set; }

    /// <summary>
    /// Gets or sets the label font size in pixels.
    /// </summary>
    [Parameter]
    public int? LabelFontSize { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var series = new EChartsSeriesOption
        {
            Type = "heatmap",
            Name = GetResolvedName(),
            Emphasis = new EChartsEmphasisOption
            {
                ItemStyle = new EChartsItemStyleOption
                {
                    BorderColor = "var(--foreground)",
                    BorderWidth = 1
                }
            }
        };

        if (ShowLabel || !string.IsNullOrEmpty(LabelFormatter))
        {
            series.Label = new EChartsLabelOption
            {
                Show = true,
                Formatter = LabelFormatter,
                Color = LabelColor,
                FontSize = LabelFontSize
            };
        }

        if (!string.IsNullOrEmpty(XKey) && !string.IsNullOrEmpty(YKey) && !string.IsNullOrEmpty(ValueKey))
        {
            var xValues = DataExtractor.ExtractValues(ParentChart?.Data, XKey);
            var yValues = DataExtractor.ExtractValues(ParentChart?.Data, YKey);
            var cellValues = DataExtractor.ExtractValues(ParentChart?.Data, ValueKey);
            var count = Math.Min(Math.Min(xValues.Count, yValues.Count), cellValues.Count);

            var data = new List<object?>(count);
            for (var i = 0; i < count; i++)
            {
                data.Add(new object?[] { xValues[i], yValues[i], cellValues[i] });
            }

            series.Data = data;
        }

        return series;
    }
}
