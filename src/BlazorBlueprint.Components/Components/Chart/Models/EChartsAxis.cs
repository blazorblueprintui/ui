using System.Text.Json.Serialization;

namespace BlazorBlueprint.Components;

internal sealed class EChartsAxisOption
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; } = "category";

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Data { get; set; }

    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; } = true;

    [JsonPropertyName("axisLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisLineOption? AxisLine { get; set; }

    [JsonPropertyName("axisTick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisTickOption? AxisTick { get; set; }

    [JsonPropertyName("axisLabel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsAxisLabelOption? AxisLabel { get; set; }

    [JsonPropertyName("splitLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsSplitLineOption? SplitLine { get; set; }

    [JsonPropertyName("boundaryGap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? BoundaryGap { get; set; }

    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Min { get; set; }

    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Max { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("inverse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Inverse { get; set; }

    [JsonPropertyName("scale")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Scale { get; set; }

    [JsonPropertyName("z")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Z { get; set; }
}

internal sealed class EChartsAxisLineOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; } = true;

    [JsonPropertyName("lineStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLineStyleOption? LineStyle { get; set; }
}

internal sealed class EChartsAxisTickOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; } = true;

    [JsonPropertyName("lineStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLineStyleOption? LineStyle { get; set; }
}

internal sealed class EChartsAxisLabelOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; } = true;

    [JsonPropertyName("inside")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Inside { get; set; }

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    [JsonPropertyName("fontSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? FontSize { get; set; }

    [JsonPropertyName("rotate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Rotate { get; set; }

    [JsonPropertyName("formatter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Formatter { get; set; }

    [JsonPropertyName("rich")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, EChartsRichStyleOption>? Rich { get; set; }
}

internal sealed class EChartsSplitLineOption
{
    [JsonPropertyName("show")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Show { get; set; } = true;

    [JsonPropertyName("lineStyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EChartsLineStyleOption? LineStyle { get; set; }
}

internal sealed class EChartsLineStyleOption
{
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}
