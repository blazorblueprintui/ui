using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A funnel series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a funnel series within a parent chart. Data is extracted as name-value
/// pairs using <see cref="SeriesBase.DataKey"/> for values and <see cref="NameKey"/> for labels.
/// Useful for visualizing conversion funnels, sales pipelines, and progressive filtering.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbFunnelChart Data="@data"&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbFunnel DataKey="value" NameKey="stage" /&gt;
/// &lt;/BbFunnelChart&gt;
/// </code>
/// </example>
public partial class BbFunnel : SeriesBase
{
    /// <summary>
    /// Gets or sets the property name used to extract category names from the chart data.
    /// </summary>
    [Parameter]
    public string? NameKey { get; set; }

    /// <summary>
    /// Gets or sets the sort order for funnel segments.
    /// </summary>
    [Parameter]
    public FunnelSort Sort { get; set; } = FunnelSort.Descending;

    /// <summary>
    /// Gets or sets the horizontal alignment of funnel segments.
    /// </summary>
    [Parameter]
    public FunnelAlign Align { get; set; } = FunnelAlign.Center;

    /// <summary>
    /// Gets or sets the gap between funnel segments in pixels.
    /// </summary>
    [Parameter]
    public int Gap { get; set; } = 2;

    /// <summary>
    /// Gets or sets the minimum segment size as a percentage or pixel value.
    /// </summary>
    [Parameter]
    public string MinSize { get; set; } = "0%";

    /// <summary>
    /// Gets or sets the maximum segment size as a percentage or pixel value.
    /// </summary>
    [Parameter]
    public string MaxSize { get; set; } = "100%";

    /// <summary>
    /// Gets or sets whether to display labels on each segment.
    /// </summary>
    [Parameter]
    public bool ShowLabels { get; set; } = true;

    /// <summary>
    /// Gets or sets the label position relative to funnel segments.
    /// </summary>
    [Parameter]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Inside;

    /// <summary>
    /// Gets or sets the ECharts label formatter string.
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
            Type = "funnel",
            Name = GetResolvedName(),
            Sort = Sort switch
            {
                FunnelSort.Ascending => "ascending",
                FunnelSort.None => "none",
                _ => "descending"
            },
            FunnelAlign = Align switch
            {
                FunnelAlign.Left => "left",
                FunnelAlign.Right => "right",
                _ => "center"
            },
            Gap = Gap > 0 ? Gap : null,
            MinSize = MinSize,
            MaxSize = MaxSize,
            Left = "10%",
            Top = "10",
            Right = "10%",
            Bottom = "10",
            Width = "80%",
            Label = new EChartsLabelOption
            {
                Show = ShowLabels,
                Position = ToEChartsPosition(LabelPosition),
                Formatter = LabelFormatter,
                Color = LabelColor,
                FontSize = LabelFontSize
            },
            ItemStyle = new EChartsItemStyleOption
            {
                BorderColor = "var(--background)",
                BorderWidth = Gap > 0 ? Gap : null
            },
            Emphasis = new EChartsEmphasisOption
            {
                Focus = "self"
            }
        };

        if (!string.IsNullOrEmpty(DataKey) && !string.IsNullOrEmpty(NameKey))
        {
            var pairs = DataExtractor.ExtractNameValuePairs(ParentChart?.Data, NameKey, DataKey);
            series.Data = new List<object?>(pairs.Select(p => (object?)p));
        }

        return series;
    }
}
