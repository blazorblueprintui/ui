using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A candlestick series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a candlestick (OHLC) series within a parent chart. Requires four data keys
/// for open, close, high, and low values. Commonly used for financial data visualization.
/// </para>
/// <para>
/// ECharts candlestick data format requires values in the order: [open, close, low, high].
/// The component handles this extraction automatically from the data keys.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbCandlestickChart Data="@data"&gt;
///     &lt;BbXAxis DataKey="date" /&gt;
///     &lt;BbYAxis /&gt;
///     &lt;BbCandlestick OpenKey="open" CloseKey="close" HighKey="high" LowKey="low" /&gt;
/// &lt;/BbCandlestickChart&gt;
/// </code>
/// </example>
public partial class BbCandlestick : SeriesBase
{
    /// <summary>
    /// Gets or sets the property name for open price values.
    /// </summary>
    [Parameter]
    public string? OpenKey { get; set; }

    /// <summary>
    /// Gets or sets the property name for close price values.
    /// </summary>
    [Parameter]
    public string? CloseKey { get; set; }

    /// <summary>
    /// Gets or sets the property name for high price values.
    /// </summary>
    [Parameter]
    public string? HighKey { get; set; }

    /// <summary>
    /// Gets or sets the property name for low price values.
    /// </summary>
    [Parameter]
    public string? LowKey { get; set; }

    /// <summary>
    /// Gets or sets the color for bullish (up) candles.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>#22c55e</c> (green). Applied when close &gt; open.
    /// </remarks>
    [Parameter]
    public string BullColor { get; set; } = "#22c55e";

    /// <summary>
    /// Gets or sets the color for bearish (down) candles.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>#ef4444</c> (red). Applied when close &lt; open.
    /// </remarks>
    [Parameter]
    public string BearColor { get; set; } = "#ef4444";

    /// <summary>
    /// Gets or sets the width of each candlestick.
    /// </summary>
    /// <remarks>
    /// Accepts a percentage string (e.g., "60%") or pixel value (e.g., "20").
    /// When null, ECharts automatically sizes candles to fit.
    /// </remarks>
    [Parameter]
    public string? BarWidth { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var series = new EChartsSeriesOption
        {
            Type = "candlestick",
            Name = GetResolvedName(),
            BarWidth = BarWidth,
            ItemStyle = new EChartsItemStyleOption
            {
                Color = BullColor,
                Color0 = BearColor,
                BorderColor = BullColor,
                BorderColor0 = BearColor
            }
        };

        if (!string.IsNullOrEmpty(OpenKey) &&
            !string.IsNullOrEmpty(CloseKey) &&
            !string.IsNullOrEmpty(HighKey) &&
            !string.IsNullOrEmpty(LowKey))
        {
            var opens = DataExtractor.ExtractValues(ParentChart?.Data, OpenKey);
            var closes = DataExtractor.ExtractValues(ParentChart?.Data, CloseKey);
            var highs = DataExtractor.ExtractValues(ParentChart?.Data, HighKey);
            var lows = DataExtractor.ExtractValues(ParentChart?.Data, LowKey);
            var count = Math.Min(Math.Min(opens.Count, closes.Count), Math.Min(highs.Count, lows.Count));

            var data = new List<object?>(count);
            for (var i = 0; i < count; i++)
            {
                // ECharts candlestick format: [open, close, low, high]
                data.Add(new object?[] { opens[i], closes[i], lows[i], highs[i] });
            }

            series.Data = data;
        }

        return series;
    }
}
