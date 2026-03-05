using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A radial bar series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a radial bar (circular progress) series within a parent chart using a polar
/// coordinate system. Supports round caps, per-item colors, labels, background tracks,
/// and an optional center label via a child <see cref="BbCenterLabel"/> component.
/// </para>
/// <para>
/// Must be placed inside a <see cref="BbRadialBarChart"/> component.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;RadialBarChart Data="@data"&gt;
///     &lt;RadialBar DataKey="visitors" NameKey="browser"&gt;
///         &lt;CenterLabel Value="925" Title="Visitors" /&gt;
///     &lt;/RadialBar&gt;
/// &lt;/RadialBarChart&gt;
/// </code>
/// </example>
public partial class BbRadialBar : SeriesBase
{
    /// <summary>
    /// Gets or sets the property name used to extract category names from the chart data.
    /// </summary>
    [Parameter]
    public string? NameKey { get; set; }

    /// <summary>
    /// Gets or sets whether bar ends are rounded.
    /// </summary>
    [Parameter]
    public bool RoundCap { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to display value labels on each bar.
    /// </summary>
    [Parameter]
    public bool ShowLabels { get; set; }

    /// <summary>
    /// Gets or sets whether to show background track bars behind the data bars.
    /// </summary>
    [Parameter]
    public bool ShowBackground { get; set; }

    private BbCenterLabel? centerLabel;

    /// <summary>
    /// Called by a child <see cref="BbCenterLabel"/> component to register itself.
    /// </summary>
    internal void SetCenterLabel(BbCenterLabel label) =>
        centerLabel = label;

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var series = new EChartsSeriesOption
        {
            Type = "bar",
            Name = GetResolvedName(),
            CoordinateSystem = "polar",
            RoundCap = RoundCap,
            ColorBy = "data",
            Emphasis = new EChartsEmphasisOption { Disabled = true },
            Label = new EChartsLabelOption
            {
                Show = false
            }
        };

        if (Stacked)
        {
            series.Stack = StackGroup;
        }

        if (ShowBackground)
        {
            series.ShowBackground = true;
            series.BackgroundStyle = new EChartsBackgroundStyleOption
            {
                Color = "var(--foreground)",
                Opacity = 0.15
            };
        }

        if (ShowLabels)
        {
            series.Label = new EChartsLabelOption
            {
                Show = true,
                Position = "middle",
                Rotate = "tangential",
                Formatter = "{b}",
                Color = "#fff",
                FontSize = 11
            };
        }

        // Build data items
        if (!string.IsNullOrEmpty(DataKey) && !string.IsNullOrEmpty(NameKey))
        {
            var pairs = DataExtractor.ExtractNameValuePairs(ParentChart?.Data, NameKey, DataKey);
            series.Data = new List<object?>(pairs.Select(p => (object?)p));
        }
        else if (!string.IsNullOrEmpty(DataKey))
        {
            series.Data = GetSeriesData();
        }

        // Apply explicit color for single-color series
        var resolvedColor = GetResolvedColor();
        if (resolvedColor != null)
        {
            series.ItemStyle = new EChartsItemStyleOption { Color = resolvedColor };
            series.ColorBy = null;
        }

        return series;
    }

    /// <inheritdoc />
    private protected override void ApplyToOption(EChartsOption option)
    {
        base.ApplyToOption(option);

        // Set radius axis category names from NameKey
        if (!string.IsNullOrEmpty(NameKey) && option.RadiusAxis != null)
        {
            var names = DataExtractor.ExtractStringValues(ParentChart?.Data, NameKey);
            if (option.RadiusAxis.Data == null || option.RadiusAxis.Data.Count == 0)
            {
                option.RadiusAxis.Data = names;
            }
        }

        // Auto-compute angle axis max from data
        if (!string.IsNullOrEmpty(DataKey) && option.AngleAxis != null && option.AngleAxis.Max == null)
        {
            var values = DataExtractor.ExtractValues(ParentChart?.Data, DataKey);
            double maxValue = 0;
            foreach (var v in values)
            {
                if (v != null)
                {
                    var d = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    if (d > maxValue)
                    {
                        maxValue = d;
                    }
                }
            }

            if (maxValue > 0)
            {
                // For stacked series, sum across all polar series to get true max
                if (Stacked && option.Series.Count > 1)
                {
                    double stackedMax = 0;
                    foreach (var existingSeries in option.Series)
                    {
                        if (existingSeries.CoordinateSystem != "polar" || existingSeries.Data == null)
                        {
                            continue;
                        }

                        foreach (var item in existingSeries.Data)
                        {
                            if (item is Dictionary<string, object?> dict && dict.TryGetValue("value", out var val) && val != null)
                            {
                                stackedMax += Convert.ToDouble(val, CultureInfo.InvariantCulture);
                            }
                            else if (item != null)
                            {
                                stackedMax += Convert.ToDouble(item, CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    option.AngleAxis.Max = stackedMax * 1.2;
                }
                else
                {
                    option.AngleAxis.Max = maxValue * 1.2;
                }
            }
        }

        // Apply center label as chart title (positioned at polar center)
        if (centerLabel != null)
        {
            ApplyCenterTitle(option);
        }
    }

    private void ApplyCenterTitle(EChartsOption option)
    {
        if (!string.IsNullOrEmpty(centerLabel!.Text))
        {
            option.Title = new EChartsTitleOption
            {
                Text = centerLabel.Text,
                Left = "center",
                Top = "middle",
                TextAlign = "center",
                TextVerticalAlign = "middle",
                TextStyle = new EChartsTextStyleOption
                {
                    FontSize = centerLabel.FontSize,
                    FontWeight = centerLabel.FontWeight,
                    Color = "var(--foreground)"
                }
            };
        }
        else
        {
            // Use rich text in a single text property so textVerticalAlign centers
            // the entire block (both value + title lines), not just the first line.
            var valueText = centerLabel.Value ?? "";
            var titleText = centerLabel.Title ?? "";
            var text = $"{{value|{valueText}}}\n{{title|{titleText}}}";

            option.Title = new EChartsTitleOption
            {
                Text = text,
                Left = "center",
                Top = "middle",
                TextAlign = "center",
                TextVerticalAlign = "middle",
                TextStyle = new EChartsTextStyleOption
                {
                    Rich = new Dictionary<string, EChartsRichStyleOption>
                    {
                        ["value"] = new EChartsRichStyleOption
                        {
                            FontSize = centerLabel.FontSize,
                            FontWeight = centerLabel.FontWeight,
                            Color = "var(--foreground)",
                            LineHeight = (int)(centerLabel.FontSize * 1.4)
                        },
                        ["title"] = new EChartsRichStyleOption
                        {
                            FontSize = 12,
                            Color = "var(--muted-foreground)",
                            LineHeight = 20
                        }
                    }
                }
            };
        }
    }
}
