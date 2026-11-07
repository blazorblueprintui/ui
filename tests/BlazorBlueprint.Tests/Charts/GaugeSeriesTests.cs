using System.Text.Json;
using BlazorBlueprint.Components;

namespace BlazorBlueprint.Tests.Charts;

#pragma warning disable BL0005 // Isolate the data-to-option pipeline without JS interop.

/// <summary>
/// ECharts reads the progress arc's colour from <c>progress.itemStyle.color</c>. Sending it as
/// <c>progress.color</c> was dropped silently, so the arc always used the first palette colour (#625).
/// </summary>
public class GaugeSeriesTests
{
    [Fact]
    public void ProgressColorColoursTheProgressArc()
    {
        var progress = Progress(new TestGauge { ShowProgress = true, Color = "#00ff00", ProgressColor = "#ff0000" });

        Assert.Equal("#ff0000", progress.GetProperty("itemStyle").GetProperty("color").GetString());
        Assert.False(progress.TryGetProperty("color", out _));
    }

    [Fact]
    public void ColorColoursTheProgressArcWhenNoProgressColorIsSet()
    {
        var progress = Progress(new TestGauge { ShowProgress = true, Color = "#00ff00" });

        Assert.Equal("#00ff00", progress.GetProperty("itemStyle").GetProperty("color").GetString());
    }

    private static JsonElement Progress(TestGauge gauge)
    {
        gauge.DataKey = "Value";
        gauge.Attach(new BbGaugeChart { Data = new[] { new Row(95) } });
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(gauge.BuildSeriesCore()));
        return json.RootElement.GetProperty("progress").Clone();
    }

    private sealed record Row(double Value);

    private sealed class TestGauge : BbGauge
    {
        public void Attach(BbChartBase chart) => ParentChart = chart;
    }
}

#pragma warning restore BL0005
