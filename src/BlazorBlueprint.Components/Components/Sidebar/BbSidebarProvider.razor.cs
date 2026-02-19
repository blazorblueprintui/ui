using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

public partial class BbSidebarProvider
{
    private SidebarContext Context { get; set; } = new();
    private IJSObjectReference? _module;
    private DotNetObjectReference<BbSidebarProvider>? _dotNetRef;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override void OnParametersSet()
    {
        // Update context when parameters change
        Context.SetVariant(Variant);
        Context.SetSide(Side);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Load the sidebar JavaScript module
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/sidebar.js");

                // Create a reference to this component for JS callbacks
                _dotNetRef = DotNetObjectReference.Create(this);

                // Initialize sidebar state from cookie if persistence is enabled
                bool? savedOpen = null;
                if (!string.IsNullOrEmpty(CookieKey))
                {
                    // Use JsonElement because JS returns bool|null and InvokeAsync<bool?> can't handle null
                    var result = await _module.InvokeAsync<JsonElement>("getSidebarState", CookieKey);
                    savedOpen = result.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => null
                    };
                }

                // Initialize context with saved state or defaults
                Context.Initialize(
                    open: savedOpen ?? DefaultOpen,
                    variant: Variant,
                    side: Side
                );

                // Set up mobile detection and keyboard shortcuts
                await _module.InvokeVoidAsync("initializeSidebar", _dotNetRef, CookieKey);

                // Subscribe to state changes for persistence
                Context.StateChanged += OnStateChanged;

                StateHasChanged();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect in Blazor Server
                Context.Initialize(open: DefaultOpen, variant: Variant, side: Side);
                StateHasChanged();
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
                Context.Initialize(open: DefaultOpen, variant: Variant, side: Side);
                StateHasChanged();
            }
        }
    }

    private async void OnStateChanged(object? sender, EventArgs e)
    {
        try
        {
            // Persist sidebar state to cookie when it changes
            if (_module != null && !string.IsNullOrEmpty(CookieKey))
            {
                try
                {
                    await _module.InvokeVoidAsync("saveSidebarState", CookieKey, Context.Open);
                }
                catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
                {
                    // Expected during circuit disconnect
                }
                catch (InvalidOperationException)
                {
                    // JS interop not available during prerendering
                }
            }

            // Notify UI of state change
            await InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
            // Component may be disposed during async operation
        }
    }

    /// <summary>
    /// Called from JavaScript when mobile state changes.
    /// </summary>
    [JSInvokable]
    public void OnMobileChange(bool isMobile) =>
        Context.SetIsMobile(isMobile);

    /// <summary>
    /// Called from JavaScript when keyboard shortcut (Ctrl/Cmd + B) is pressed.
    /// </summary>
    [JSInvokable]
    public void OnToggleShortcut()
    {
        Context.ToggleSidebar();
        StateHasChanged(); // Force re-render after toggle
    }

    public async ValueTask DisposeAsync()
    {
        if (Context != null)
        {
            Context.StateChanged -= OnStateChanged;
        }

        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("cleanup");
                await _module.DisposeAsync();
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

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}
