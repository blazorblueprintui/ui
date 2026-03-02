using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Demo.Shared;

public partial class MainLayout : LayoutComponentBase, IAsyncDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    private IJSObjectReference? utilsModule;

    protected override void OnInitialized() =>
        NavigationManager.LocationChanged += OnLocationChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            utilsModule = await Js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Demo.Shared/js/demo-utils.js");
        }
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        try
        {
            if (utilsModule != null)
            {
                await utilsModule.InvokeVoidAsync("scrollMainContentToTop");
            }
        }
        catch (Exception ex) when (ex is ObjectDisposedException or JSDisconnectedException or TaskCanceledException)
        {
            // Component disposed during navigation
        }
    }

    public async ValueTask DisposeAsync()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;

        if (utilsModule != null)
        {
            try
            {
                await utilsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        GC.SuppressFinalize(this);
    }
}
