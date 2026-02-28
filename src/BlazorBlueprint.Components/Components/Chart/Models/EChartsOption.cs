using System.Text.Json.Serialization;

namespace BlazorBlueprint.Components;

internal sealed class EChartsOption
{
    [JsonPropertyName("animation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Animation { get; set; } = true;

    [JsonPropertyName("xAxis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisOption? XAxis { get; set; }

    [JsonPropertyName("yAxis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisOption? YAxis { get; set; }

    [JsonPropertyName("grid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsGridOption? Grid { get; set; }

    [JsonPropertyName("tooltip")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsTooltipOption? Tooltip { get; set; }

    [JsonPropertyName("legend")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLegendOption? Legend { get; set; }

    [JsonPropertyName("series")]
    public List<EChartsSeriesOption> Series { get; set; } = [];

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<object>? Color { get; set; }

    [JsonPropertyName("radar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsRadarOption? Radar { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsTitleOption? Title { get; set; }

    [JsonPropertyName("polar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsPolarOption? Polar { get; set; }

    [JsonPropertyName("angleAxis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAngleAxisOption? AngleAxis { get; set; }

    [JsonPropertyName("radiusAxis")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsRadiusAxisOption? RadiusAxis { get; set; }

    [JsonPropertyName("visualMap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsVisualMapOption? VisualMap { get; set; }
}
