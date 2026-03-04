using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A bar series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a bar series within a parent chart. Supports configurable border radius,
/// bar width, stacking, and fill colors including gradients via child <see cref="BbFill"/> components.
/// </para>
/// <para>
/// Must be placed inside a chart component that provides a <see cref="BbChartBase"/> cascading value.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BarChart Data="@data"&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis /&gt;
///     &lt;Bar DataKey="desktop" Name="Desktop" BorderRadius="6" /&gt;
///     &lt;Bar DataKey="mobile" Name="Mobile" /&gt;
/// &lt;/BarChart&gt;
/// </code>
/// </example>
public partial class BbBar : SeriesBase
{
    /// <summary>
    /// Gets or sets the border radius for bar corners in pixels.
    /// </summary>
    /// <remarks>
    /// Applied to the leading end of bars (top for vertical, right for horizontal).
    /// When null, defaults to 4 for non-stacked bars and 0 for stacked bars.
    /// For negative values, the radius is automatically applied to the opposite end.
    /// </remarks>
    [Parameter]
    public int? BorderRadius { get; set; }

    /// <summary>
    /// Gets or sets the width of each bar.
    /// </summary>
    /// <remarks>
    /// Accepts a percentage string (e.g., "60%") or pixel value (e.g., "20").
    /// When null, ECharts automatically sizes bars to fit.
    /// </remarks>
    [Parameter]
    public string? BarWidth { get; set; }

    /// <summary>
    /// Gets or sets whether data labels are displayed on each bar.
    /// </summary>
    [Parameter]
    public bool ShowLabel { get; set; }

    /// <summary>
    /// Gets or sets the position of the data label relative to the bar.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="LabelPosition.Top"/> for vertical bars.
    /// </remarks>
    [Parameter]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Top;

    /// <summary>
    /// Gets or sets the label formatter string.
    /// </summary>
    /// <remarks>
    /// ECharts formatter string. Use <c>{b}</c> for category name, <c>{c}</c> for value.
    /// When null, the default value label is shown.
    /// </remarks>
    [Parameter]
    public string? LabelFormatter { get; set; }

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    [Parameter]
    public string? LabelColor { get; set; }

    /// <summary>
    /// Gets or sets the label font size.
    /// </summary>
    [Parameter]
    public int? LabelFontSize { get; set; }

    /// <summary>
    /// Gets or sets the property name used to extract per-item fill colors from the chart data.
    /// </summary>
    /// <remarks>
    /// When set, each data item's fill color is read from this property, enabling
    /// individual bar coloring. The property should contain CSS color values or
    /// CSS variable references (e.g., "var(--chart-1)").
    /// </remarks>
    [Parameter]
    public string? FillKey { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();
        var effectiveColor = GetResolvedFillColor() ?? resolvedColor;
        var isHorizontal = ParentChart?.SwapAxes == true;
        var radius = BorderRadius ?? (Stacked ? 0 : 4);
        var rawData = GetSeriesData();

        // Determine if we need per-item data objects
        var perItemColors = !string.IsNullOrEmpty(FillKey)
            ? DataExtractor.ExtractValues(ParentChart?.Data, FillKey)
            : null;
        var hasNegative = HasNegativeValues(rawData);
        var needsPerItemData = hasNegative || perItemColors != null;

        var series = new EChartsSeriesOption
        {
            Type = "bar",
            Name = GetResolvedName(),
            BarWidth = BarWidth,
            Emphasis = new EChartsEmphasisOption { Disabled = true }
        };

        if (needsPerItemData)
        {
            series.Data = BuildPerItemData(rawData, perItemColors, effectiveColor, radius, isHorizontal);
            series.ItemStyle = new EChartsItemStyleOption
            {
                Color = effectiveColor
            };
        }
        else
        {
            series.Data = rawData;
            series.ItemStyle = new EChartsItemStyleOption
            {
                BorderRadius = GetBorderRadiusArray(radius, isHorizontal, false),
                Color = effectiveColor
            };
        }

        if (ShowLabel)
        {
            series.Label = new EChartsLabelOption
            {
                Show = true,
                Position = ToEChartsPosition(LabelPosition),
                Formatter = LabelFormatter,
                Color = LabelColor ?? "var(--foreground)",
                FontSize = LabelFontSize,
                TextBorderWidth = 0
            };
        }

        if (Stacked)
        {
            series.Stack = StackGroup;
        }

        return series;
    }

    private static List<object?> BuildPerItemData(
        List<object?> rawData,
        List<object?>? perItemColors,
        object? defaultColor,
        int radius,
        bool isHorizontal)
    {
        var itemData = new List<object?>(rawData.Count);

        for (var i = 0; i < rawData.Count; i++)
        {
            var value = rawData[i];
            var isNeg = IsNegative(value);
            var itemRadius = GetBorderRadiusArray(radius, isHorizontal, isNeg);
            var itemColor = (perItemColors != null && i < perItemColors.Count)
                ? perItemColors[i]
                : defaultColor;

            itemData.Add(new Dictionary<string, object?>
            {
                ["value"] = value,
                ["itemStyle"] = new Dictionary<string, object?>
                {
                    ["borderRadius"] = itemRadius,
                    ["color"] = itemColor
                }
            });
        }

        return itemData;
    }

    private static int[] GetBorderRadiusArray(int radius, bool isHorizontal, bool isNegative)
    {
        if (radius == 0)
        {
            return [0, 0, 0, 0];
        }

        if (isHorizontal)
        {
            // Horizontal: positive rounds right end, negative rounds left end
            return isNegative
                ? [radius, 0, 0, radius]
                : [0, radius, radius, 0];
        }

        // Vertical: positive rounds top, negative rounds bottom
        return isNegative
            ? [0, 0, radius, radius]
            : [radius, radius, 0, 0];
    }

    private static bool HasNegativeValues(List<object?> data)
    {
        foreach (var value in data)
        {
            if (IsNegative(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNegative(object? value)
    {
        return value switch
        {
            int i => i < 0,
            long l => l < 0,
            double d => d < 0,
            float f => f < 0,
            decimal m => m < 0,
            _ => false
        };
    }
}
