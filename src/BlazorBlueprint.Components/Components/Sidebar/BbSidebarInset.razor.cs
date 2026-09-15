using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

public partial class BbSidebarInset : IAsyncDisposable
{
    private IJSObjectReference? module;
    private ElementReference mainRef;
    private bool disposed;
    private bool jsReady;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter]
    private SidebarContext? Context { get; set; }

    /// <summary>
    /// The main content to render.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the inset.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// When true, automatically scrolls to the top when the route changes.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ResetScrollOnNavigation { get; set; } = true;

    /// <summary>
    /// Additional attributes to apply to the main element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        if (ResetScrollOnNavigation)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && ResetScrollOnNavigation)
        {
            try
            {
                module = await ComponentModules.GetCoreAsync(JSRuntime);
                jsReady = true;
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
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (disposed || !jsReady || module == null)
        {
            return;
        }

        try
        {
            await InvokeAsync(async () =>
            {
                try
                {
                    // Namespaced, because this module is the bb-components-core bundle rather
                    // than sidebar-inset.js on its own. The bundle re-exports each module under
                    // its file name in camelCase, so the bare identifier resolves to nothing.
                    await module!.InvokeVoidAsync("sidebarInset.scrollToTop", mainRef);
                }
                catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
                {
                    // Circuit disconnected, or the function is not there. Scrolling to the top of
                    // a new page is a courtesy; JSException is caught alongside the rest because
                    // letting it escape an async void handler takes the whole circuit down, and
                    // no failure of this call is worth that.
                }
                catch (InvalidOperationException)
                {
                    // JS interop not available
                }
            });
        }
        catch (Exception)
        {
            // async void: nothing awaits this, so an exception that escapes has no caller to reach and
            // Blazor Server treats it as fatal — the circuit closes and the user sees the reconnect
            // overlay. Everything this method does is best-effort, and none of it is worth that.
        }
    }

    private string GetClasses()
    {
        var baseClasses = "relative flex h-full flex-1 flex-col bg-background focus:outline-none";

        // Floating variant margins - push content when sidebar is visible
        // When sidebar is closed (hidden), remove margins
        var floatingClasses = Context?.Side == SidebarSide.Right
            ? "md:peer-data-[variant=floating]:mr-2 md:peer-data-[variant=floating]:peer-data-[state=collapsed]:mr-[calc(var(--sidebar-width-icon)+0.5rem+0.5rem)] md:peer-data-[variant=floating]:peer-data-[state=expanded]:mr-[calc(var(--sidebar-width)+0.5rem+0.5rem)] md:peer-data-[variant=floating]:peer-data-[state=closed]:mr-0"
            : "md:peer-data-[variant=floating]:ml-2 md:peer-data-[variant=floating]:peer-data-[state=collapsed]:ml-[calc(var(--sidebar-width-icon)+0.5rem+0.5rem)] md:peer-data-[variant=floating]:peer-data-[state=expanded]:ml-[calc(var(--sidebar-width)+0.5rem+0.5rem)] md:peer-data-[variant=floating]:peer-data-[state=closed]:ml-0";

        // Add margin transitions
        var transitionClasses = "transition-[margin] duration-200 ease-linear";

        // Inset variant specific styling - margin on all sides, rounded corners, shadow, and calculated height for margins
        var insetRoundingClasses = "md:peer-data-[variant=inset]:m-2 md:peer-data-[variant=inset]:h-[calc(100%-1rem)] md:peer-data-[variant=inset]:min-h-0 md:peer-data-[variant=inset]:rounded-xl md:peer-data-[variant=inset]:shadow md:peer-data-[variant=inset]:bg-background";

        return ClassNames.cn(
            baseClasses,
            floatingClasses,
            transitionClasses,
            insetRoundingClasses,
            Class
        );
    }

    public ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return ValueTask.CompletedTask;
        }
        disposed = true;

        if (ResetScrollOnNavigation)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
