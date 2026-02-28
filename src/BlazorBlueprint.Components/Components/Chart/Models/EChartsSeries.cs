using System.Text.Json.Serialization;

namespace BlazorBlueprint.Components;

internal sealed class EChartsSeriesOption
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; } = "line";

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object?>? Data { get; set; }

    [JsonPropertyName("stack")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Stack { get; set; }

    [JsonPropertyName("smooth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Smooth { get; set; }

    [JsonPropertyName("step")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Step { get; set; }

    [JsonPropertyName("symbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Symbol { get; set; }

    [JsonPropertyName("symbolSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SymbolSize { get; set; }

    [JsonPropertyName("showSymbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowSymbol { get; set; }

    [JsonPropertyName("lineStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLineStyleOption? LineStyle { get; set; }

    [JsonPropertyName("areaStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAreaStyleOption? AreaStyle { get; set; }

    [JsonPropertyName("itemStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsItemStyleOption? ItemStyle { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLabelOption? Label { get; set; }

    [JsonPropertyName("emphasis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsEmphasisOption? Emphasis { get; set; }

    [JsonPropertyName("radius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Radius { get; set; }

    [JsonPropertyName("center")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Center { get; set; }

    [JsonPropertyName("coordinateSystem")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CoordinateSystem { get; set; }

    [JsonPropertyName("radarIndex")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RadarIndex { get; set; }

    [JsonPropertyName("barWidth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BarWidth { get; set; }

    [JsonPropertyName("barGap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BarGap { get; set; }

    [JsonPropertyName("roundCap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? RoundCap { get; set; }

    [JsonPropertyName("encode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Encode { get; set; }

    [JsonPropertyName("labelLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLabelLineOption? LabelLine { get; set; }

    [JsonPropertyName("selectedMode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? SelectedMode { get; set; }

    [JsonPropertyName("showBackground")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ShowBackground { get; set; }

    [JsonPropertyName("backgroundStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsBackgroundStyleOption? BackgroundStyle { get; set; }

    [JsonPropertyName("colorBy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ColorBy { get; set; }

    // Funnel properties

    [JsonPropertyName("sort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sort { get; set; }

    [JsonPropertyName("funnelAlign")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FunnelAlign { get; set; }

    [JsonPropertyName("left")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Left { get; set; }

    [JsonPropertyName("top")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Top { get; set; }

    [JsonPropertyName("right")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Right { get; set; }

    [JsonPropertyName("bottom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Bottom { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Width { get; set; }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Height { get; set; }

    [JsonPropertyName("minSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MinSize { get; set; }

    [JsonPropertyName("maxSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MaxSize { get; set; }

    [JsonPropertyName("gap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Gap { get; set; }

    // Gauge properties

    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; }

    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; }

    [JsonPropertyName("startAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StartAngle { get; set; }

    [JsonPropertyName("endAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EndAngle { get; set; }

    [JsonPropertyName("splitNumber")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SplitNumber { get; set; }

    [JsonPropertyName("progress")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugeProgressOption? Progress { get; set; }

    [JsonPropertyName("axisLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugeAxisLineOption? AxisLine { get; set; }

    [JsonPropertyName("axisTick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisTickOption? AxisTick { get; set; }

    [JsonPropertyName("splitLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugeSplitLineOption? SplitLine { get; set; }

    [JsonPropertyName("pointer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugePointerOption? Pointer { get; set; }

    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugeDetailOption? Detail { get; set; }

    [JsonPropertyName("anchor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGaugeAnchorOption? Anchor { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsSeriesTitleOption? Title { get; set; }
}

internal sealed class EChartsBackgroundStyleOption
{
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    [JsonPropertyName("borderColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor { get; set; }

    [JsonPropertyName("borderWidth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BorderWidth { get; set; }

    [JsonPropertyName("opacity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Opacity { get; set; }
}

internal sealed class EChartsAreaStyleOption
{
    [JsonPropertyName("opacity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Opacity { get; set; }

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Color { get; set; }
}

internal sealed class EChartsItemStyleOption
{
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Color { get; set; }

    [JsonPropertyName("borderRadius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? BorderRadius { get; set; }

    [JsonPropertyName("borderColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor { get; set; }

    [JsonPropertyName("borderWidth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BorderWidth { get; set; }

    [JsonPropertyName("opacity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Opacity { get; set; }

    [JsonPropertyName("color0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color0 { get; set; }

    [JsonPropertyName("borderColor0")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BorderColor0 { get; set; }
}

internal sealed class EChartsLabelOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; }

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    [JsonPropertyName("fontSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FontSize { get; set; }

    [JsonPropertyName("fontWeight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FontWeight { get; set; }

    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Position { get; set; }

    [JsonPropertyName("formatter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Formatter { get; set; }

    [JsonPropertyName("rotate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Rotate { get; set; }

    [JsonPropertyName("rich")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Rich { get; set; }
}

internal sealed class EChartsEmphasisOption
{
    [JsonPropertyName("itemStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsItemStyleOption? ItemStyle { get; set; }

    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLabelOption? Label { get; set; }

    [JsonPropertyName("focus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Focus { get; set; }

    [JsonPropertyName("scale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Scale { get; set; }

    [JsonPropertyName("scaleSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ScaleSize { get; set; }
}

internal sealed class EChartsLabelLineOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; }
}
