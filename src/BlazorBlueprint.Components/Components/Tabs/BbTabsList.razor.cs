using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

public partial class BbTabsList : IAsyncDisposable
{
    private ElementReference _containerRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<BbTabsList>? _dotNetRef;
    private bool _isOverflowing;
    private bool _disposed;
    private readonly List<(string Value, string Label)> _responsiveTriggers = new();
    private string _componentId = Guid.NewGuid().ToString("N")[..8];

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// The child content to render within the tabs list.
    /// Should contain TabsTrigger components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the tabs list.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// When true, collapses tabs into a Select dropdown when they overflow their container.
    /// </summary>
    [Parameter]
    public bool Responsive { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the responsive Select fallback.
    /// </summary>
    [Parameter]
    public string? SelectClass { get; set; }

    /// <summary>
    /// Cascading parameter to receive the tabs context from the Primitives layer.
    /// </summary>
    [CascadingParameter]
    public TabsContext Context { get; set; } = null!;

    private string CssClass => ClassNames.cn(
        "bb:inline-flex bb:h-10 bb:items-center bb:justify-center bb:rounded-md bb:bg-muted bb:p-1 bb:text-muted-foreground",
        Class
    );

    // Use invisible (not hidden/display:none) so the tablist keeps its measured width
    // for the ResizeObserver. pointer-events-none prevents interaction while invisible.
    private string TabListVisibilityClass => _isOverflowing ? "bb:invisible bb:pointer-events-none" : "";

    // The select is absolutely positioned over the tablist area to avoid affecting layout.
    // When not overflowing, it's hidden entirely.
    private string SelectWrapperClass => _isOverflowing
        ? "bb:absolute bb:inset-x-0 bb:top-0"
        : "bb:hidden";

    private string SelectCssClass => ClassNames.cn(
        "bb:w-full",
        SelectClass
    );

    private SelectOption<string>[] SelectOptions =>
        _responsiveTriggers.Select(t => new SelectOption<string>(t.Value, t.Label)).ToArray();

    internal void RegisterResponsiveTrigger(string value, string label)
    {
        if (!_responsiveTriggers.Any(t => t.Value == value))
        {
            _responsiveTriggers.Add((value, label));
        }
    }

    internal void UnregisterResponsiveTrigger(string value) =>
        _responsiveTriggers.RemoveAll(t => t.Value == value);

    private void HandleSelectChange(string? value)
    {
        if (value is not null)
        {
            Context.SetActiveTab(value);
        }
    }

    [JSInvokable]
    public void OnOverflowChange(bool isOverflowing)
    {
        if (_isOverflowing != isOverflowing)
        {
            _isOverflowing = isOverflowing;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Responsive)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _jsModule = await JsModules.GetAsync(JSRuntime, "./_content/BlazorBlueprint.Components/js/responsive-tabs.js");
                await _jsModule.InvokeVoidAsync("initialize", _dotNetRef, _componentId, _containerRef);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);

        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose", _componentId);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        _dotNetRef?.Dispose();
    }
}
