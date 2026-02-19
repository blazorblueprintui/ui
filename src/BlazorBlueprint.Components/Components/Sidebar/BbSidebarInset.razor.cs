using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

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
                module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/sidebar-inset.js");
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
                    await module!.InvokeVoidAsync("scrollToTop", mainRef);
                }
                catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
                {
                    // Circuit disconnected
                }
                catch (InvalidOperationException)
                {
                    // JS interop not available
                }
            });
        }
        catch (ObjectDisposedException)
        {
            // Component disposed during async operation
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

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;

        if (ResetScrollOnNavigation)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        if (module != null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        GC.SuppressFinalize(this);
    }
}
