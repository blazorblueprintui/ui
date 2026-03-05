using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A pie/donut series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a pie or donut series within a parent chart. Data is extracted as name-value
/// pairs using <see cref="SeriesBase.DataKey"/> for values and <see cref="NameKey"/> for labels.
/// Set <see cref="InnerRadius"/> greater than 0 to create a donut chart.
/// </para>
/// <para>
/// Supports an optional center label via a child <see cref="BbCenterLabel"/> component
/// (only visible when <see cref="InnerRadius"/> is greater than 0).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PieChart Data="@data"&gt;
///     &lt;Pie DataKey="visitors" NameKey="browser" InnerRadius="60" ShowLabels="true" /&gt;
/// &lt;/PieChart&gt;
/// </code>
/// </example>
public partial class BbPie : SeriesBase
{
    /// <summary>
    /// Gets or sets the property name used to extract category names from the chart data.
    /// </summary>
    /// <remarks>
    /// Used together with <see cref="SeriesBase.DataKey"/> to extract name-value pairs
    /// for pie slices. For example, NameKey="browser" and DataKey="visitors".
    /// </remarks>
    [Parameter]
    public string? NameKey { get; set; }

    /// <summary>
    /// Gets or sets the outer radius as a percentage of the chart container.
    /// </summary>
    [Parameter]
    public int OuterRadius { get; set; } = 80;

    /// <summary>
    /// Gets or sets the inner radius as a percentage of the chart container.
    /// </summary>
    /// <remarks>
    /// Set to 0 for a full pie chart or greater than 0 for a donut chart.
    /// </remarks>
    [Parameter]
    public int InnerRadius { get; set; }

    /// <summary>
    /// Gets or sets the padding angle between slices in degrees.
    /// </summary>
    /// <remarks>
    /// Creates visual separation between pie slices by drawing a border
    /// colored to match the chart background. Default is 2 pixels.
    /// </remarks>
    [Parameter]
    public int PaddingAngle { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to display labels on each slice.
    /// </summary>
    [Parameter]
    public bool ShowLabels { get; set; }

    /// <summary>
    /// Gets or sets the label position relative to pie slices.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="Components.LabelPosition.Outside"/>. Use <see cref="Components.LabelPosition.Inside"/>
    /// to render labels within slice areas.
    /// </remarks>
    [Parameter]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Outside;

    /// <summary>
    /// Gets or sets the ECharts label formatter string.
    /// </summary>
    /// <remarks>
    /// Use ECharts template variables: {b} for name, {c} for value, {d} for percentage.
    /// Setting this automatically enables labels.
    /// </remarks>
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

    /// <summary>
    /// Gets or sets whether to show leader lines connecting labels to slices.
    /// </summary>
    /// <remarks>
    /// Defaults to true when labels are outside, false when inside.
    /// </remarks>
    [Parameter]
    public bool? ShowLabelLine { get; set; }

    /// <summary>
    /// Gets or sets the index of the pre-selected (active) slice.
    /// </summary>
    /// <remarks>
    /// The selected slice is visually offset from center to indicate an active state.
    /// Uses ECharts selectedMode="single" with the specified data item pre-selected.
    /// </remarks>
    [Parameter]
    public int? ActiveIndex { get; set; }

    private BbCenterLabel? centerLabel;

    /// <summary>
    /// Called by a child <see cref="BbCenterLabel"/> component to register itself.
    /// </summary>
    internal void SetCenterLabel(BbCenterLabel label) =>
        centerLabel = label;

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var hasCenterLabel = centerLabel != null && InnerRadius > 0;
        var showLabel = !hasCenterLabel && (ShowLabels || !string.IsNullOrEmpty(LabelFormatter));

        var series = new EChartsSeriesOption
        {
            Type = "pie",
            Name = GetResolvedName(),
            Radius = InnerRadius > 0
                ? new[] { $"{InnerRadius}%", $"{OuterRadius}%" }
                : (object)$"{OuterRadius}%",
            ItemStyle = new EChartsItemStyleOption
            {
                BorderColor = "var(--background)",
                BorderWidth = PaddingAngle
            },
            Emphasis = new EChartsEmphasisOption
            {
                Disabled = true,
                Scale = false
            }
        };

        if (ActiveIndex.HasValue)
        {
            series.SelectedMode = "single";
        }

        // Configure labels
        if (hasCenterLabel)
        {
            ApplyCenterLabel(series);
        }
        else
        {
            series.Label = new EChartsLabelOption
            {
                Show = showLabel,
                Position = ToEChartsPosition(LabelPosition),
                Formatter = LabelFormatter,
                Color = LabelColor ?? "var(--foreground)",
                FontSize = LabelFontSize,
                TextBorderWidth = 0
            };
            series.LabelLine = showLabel
                ? new EChartsLabelLineOption
                {
                    Show = ShowLabelLine ?? (LabelPosition != LabelPosition.Inside)
                }
                : null;
        }

        // Extract data
        if (!string.IsNullOrEmpty(DataKey) && !string.IsNullOrEmpty(NameKey))
        {
            var pairs = DataExtractor.ExtractNameValuePairs(ParentChart?.Data, NameKey, DataKey);

            if (ActiveIndex.HasValue && ActiveIndex.Value >= 0 && ActiveIndex.Value < pairs.Count)
            {
                pairs[ActiveIndex.Value]["selected"] = true;
            }

            series.Data = new List<object?>(pairs.Select(p => (object?)p));
        }

        return series;
    }

    private void ApplyCenterLabel(EChartsSeriesOption series)
    {
        if (!string.IsNullOrEmpty(centerLabel!.Text))
        {
            series.Label = new EChartsLabelOption
            {
                Show = true,
                Position = "center",
                FontSize = centerLabel.FontSize,
                FontWeight = centerLabel.FontWeight,
                Color = "var(--foreground)",
                Formatter = centerLabel.Text
            };
        }
        else
        {
            var formatterParts = new List<string>();
            if (!string.IsNullOrEmpty(centerLabel.Value))
            {
                formatterParts.Add($"{{value|{centerLabel.Value}}}");
            }

            if (!string.IsNullOrEmpty(centerLabel.Title))
            {
                formatterParts.Add($"{{title|{centerLabel.Title}}}");
            }

            series.Label = new EChartsLabelOption
            {
                Show = true,
                Position = "center",
                Formatter = string.Join("\n", formatterParts),
                Rich = new Dictionary<string, object>
                {
                    ["value"] = new Dictionary<string, object>
                    {
                        ["fontSize"] = centerLabel.FontSize,
                        ["fontWeight"] = centerLabel.FontWeight,
                        ["color"] = "var(--foreground)",
                        ["lineHeight"] = centerLabel.FontSize + 8
                    },
                    ["title"] = new Dictionary<string, object>
                    {
                        ["fontSize"] = 12,
                        ["color"] = "var(--muted-foreground)",
                        ["lineHeight"] = 20
                    }
                }
            };
        }

        series.LabelLine = new EChartsLabelLineOption { Show = false };
    }
}
