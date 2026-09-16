using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

public partial class BbResponsiveNavProvider
{
    private ResponsiveNavContext Context { get; set; } = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<BbResponsiveNavProvider>? _dotNetRef;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Load the responsive nav JavaScript module
                _module = await JsModules.GetAsync(JSRuntime, "./_content/BlazorBlueprint.Components/js/responsive-nav.js");

                // Create a reference to this component for JS callbacks
                _dotNetRef = DotNetObjectReference.Create(this);

                // Initialize mobile detection
                await _module.InvokeVoidAsync("initialize", _dotNetRef);

                // Subscribe to state changes
                Context.StateChanged += OnStateChanged;

                StateHasChanged();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect in Blazor Server
                StateHasChanged();
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
                StateHasChanged();
            }
        }
    }

    private async void OnStateChanged(object? sender, EventArgs e)
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception)
        {
            // async void: nothing awaits this, so an exception that escapes has no caller to reach and
            // Blazor Server treats it as fatal — the circuit closes and the user sees the reconnect
            // overlay. Everything this method does is best-effort, and none of it is worth that.
        }
    }

    /// <summary>
    /// Called from JavaScript when mobile state changes.
    /// </summary>
    [JSInvokable]
    public void OnMobileChange(bool isMobile) =>
        Context.SetIsMobile(isMobile);

    public async ValueTask DisposeAsync()
    {
        Context?.StateChanged -= OnStateChanged;

        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("cleanup");
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}
