using BlazorBlueprint.Demo.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Demo.Shared;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private CollapsibleStateService StateService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    // State for each collapsible menu section
    private bool _primitivesMenuOpen;
    private bool _componentsMenuOpen;
    private bool _chartsMenuOpen;
    private bool _iconsMenuOpen;
    private bool _guidesMenuOpen;

    // State keys for localStorage
    private const string PrimitivesMenuKey = "sidebar-primitives-menu";
    private const string ComponentsMenuKey = "sidebar-components-menu";
    private const string ChartsMenuKey = "sidebar-charts-menu";
    private const string IconsMenuKey = "sidebar-icons-menu";
    private const string GuidesMenuKey = "sidebar-guides-menu";

    protected override void OnInitialized() =>
        NavigationManager.LocationChanged += OnLocationChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved state from localStorage on first render
            _primitivesMenuOpen = await StateService.GetStateAsync(PrimitivesMenuKey, defaultValue: false);
            _componentsMenuOpen = await StateService.GetStateAsync(ComponentsMenuKey, defaultValue: false);
            _chartsMenuOpen = await StateService.GetStateAsync(ChartsMenuKey, defaultValue: false);
            _iconsMenuOpen = await StateService.GetStateAsync(IconsMenuKey, defaultValue: false);
            _guidesMenuOpen = await StateService.GetStateAsync(GuidesMenuKey, defaultValue: false);

            // Trigger re-render with loaded state
            StateHasChanged();
        }
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        try
        {
            await Js.InvokeVoidAsync("eval", "document.getElementById('main-content')?.scrollTo(0, 0)");
        }
        catch (ObjectDisposedException)
        {
            // Component disposed during navigation
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        GC.SuppressFinalize(this);
    }

    // Event handlers for state changes
    private async Task OnPrimitivesMenuOpenChanged(bool isOpen)
    {
        _primitivesMenuOpen = isOpen;
        await StateService.SetStateAsync(PrimitivesMenuKey, isOpen);
    }

    private async Task OnComponentsMenuOpenChanged(bool isOpen)
    {
        _componentsMenuOpen = isOpen;
        await StateService.SetStateAsync(ComponentsMenuKey, isOpen);
    }

    private async Task OnChartsMenuOpenChanged(bool isOpen)
    {
        _chartsMenuOpen = isOpen;
        await StateService.SetStateAsync(ChartsMenuKey, isOpen);
    }

    private async Task OnIconsMenuOpenChanged(bool isOpen)
    {
        _iconsMenuOpen = isOpen;
        await StateService.SetStateAsync(IconsMenuKey, isOpen);
    }

    private async Task OnGuidesMenuOpenChanged(bool isOpen)
    {
        _guidesMenuOpen = isOpen;
        await StateService.SetStateAsync(GuidesMenuKey, isOpen);
    }
}
