using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

public abstract partial class BbChartBase : ComponentBase, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private IJSObjectReference? jsModule;
    private bool jsInitialized;
    private bool disposed;
    private readonly string chartId = Guid.NewGuid().ToString("N");
    private readonly List<IChartComponent> registeredComponents = [];
    private bool hasRenderedOnce;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public object? Data { get; set; }

    [Parameter]
    public ChartConfig? Config { get; set; }

    [Parameter]
    public string Height { get; set; } = "350px";

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool EnableAnimations { get; set; } = true;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    internal abstract string DataSlot { get; }

    internal abstract string SeriesType { get; }

    /// <summary>
    /// When true, XAxis composable writes to option.YAxis and YAxis composable writes to option.XAxis.
    /// Used by BarChart when Horizontal is true to swap category/value axes.
    /// </summary>
    internal virtual bool SwapAxes => false;

    internal abstract void ApplyChartDefaults(EChartsOption option);

    internal void RegisterComponent(IChartComponent component)
    {
        registeredComponents.Add(component);
        if (hasRenderedOnce)
        {
            StateHasChanged();
        }
    }

    internal void UnregisterComponent(IChartComponent component) =>
        registeredComponents.Remove(component);

    private EChartsOption BuildOption()
    {
        var option = new EChartsOption
        {
            Animation = EnableAnimations
        };

        if (!string.IsNullOrEmpty(Title))
        {
            option.Title = new EChartsTitleOption
            {
                Text = Title
            };
        }

        ApplyChartDefaults(option);

        foreach (var component in registeredComponents)
        {
            component.ApplyTo(option);
        }

        if (option.Color == null || option.Color.Count == 0)
        {
            option.Color = new List<object>(ChartColor.DefaultColors);
        }

        AdjustLayoutForLegend(option);

        return option;
    }

    /// <summary>
    /// Adjusts chart layout when a visible legend is present.
    /// The legend uses <c>type: "scroll"</c> so its height is bounded regardless of item count.
    /// We apply a uniform offset to push chart content away from the legend edge.
    /// </summary>
    private static void AdjustLayoutForLegend(EChartsOption option)
    {
        if (option.Legend?.Show != true)
        {
            return;
        }

        var legendAtBottom = option.Legend.Bottom != null;
        var legendAtTop = option.Legend.Top != null;

        if (!legendAtBottom && !legendAtTop)
        {
            return;
        }

        // The scroll legend has a fixed single-row height (~30px).
        // Shift chart content away from that edge by a consistent amount.
        const string gridPadding = "30";
        var centerY = legendAtBottom ? "45%" : "55%";

        // Grid-based charts (Line, Bar, Area): add margin to the grid
        if (option.Grid != null)
        {
            if (legendAtBottom)
            {
                option.Grid.Bottom = gridPadding;
            }
            else
            {
                option.Grid.Top = gridPadding;
            }
        }

        // Center-based charts (Pie): shift center and shrink radius
        foreach (var series in option.Series)
        {
            if (series.Type == "pie" && series.Center == null)
            {
                series.Center = ["50%", centerY];
                ShrinkPieRadius(series, 0.85);
            }
        }

        // Radar: shift center and shrink radius
        if (option.Radar != null && option.Radar.Center == null)
        {
            option.Radar.Center = ["50%", centerY];
            option.Radar.Radius = "65%";
        }
    }

    private static void ShrinkPieRadius(EChartsSeriesOption series, double scale)
    {
        if (series.Radius is object[] arr)
        {
            if (arr.Length == 2 && arr[1] is string outer && outer.EndsWith('%'))
            {
                var val = int.Parse(outer.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture);
                arr[1] = $"{(int)(val * scale)}%";
            }
        }
        else if (series.Radius is string single && single.EndsWith('%'))
        {
            var val = int.Parse(single.TrimEnd('%'), System.Globalization.CultureInfo.InvariantCulture);
            series.Radius = $"{(int)(val * scale)}%";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            hasRenderedOnce = true;
            await InitializeChartAsync();
        }
        else
        {
            await UpdateChartAsync();
        }
    }

    private async Task InitializeChartAsync()
    {
        try
        {
            jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/echarts-renderer.js");

            var option = BuildOption();
            var serialized = SerializeOption(option);

            await jsModule.InvokeVoidAsync("initialize", chartId, serialized);
            jsInitialized = true;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // Expected during circuit disconnect in Blazor Server
        }
        catch (InvalidOperationException)
        {
            // JS interop not available during prerendering
        }
    }

    private async Task UpdateChartAsync()
    {
        if (disposed || !jsInitialized || jsModule == null)
        {
            return;
        }

        try
        {
            var option = BuildOption();
            var serialized = SerializeOption(option);

            await jsModule.InvokeVoidAsync("update", chartId, serialized);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // Expected during circuit disconnect in Blazor Server
        }
        catch (InvalidOperationException)
        {
            // JS interop not available during prerendering
        }
    }

    private static JsonElement SerializeOption(EChartsOption option)
    {
        var json = JsonSerializer.Serialize(option, SerializerOptions);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private string ContainerCssClass => ClassNames.cn("w-full", Class);

    private string ContainerStyle => $"height: {Height}; width: {Width};";

    public async ValueTask DisposeAsync()
    {
        disposed = true;

        if (jsModule != null)
        {
            try
            {
                await jsModule.InvokeVoidAsync("dispose", chartId);
                await jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        GC.SuppressFinalize(this);
    }
}
