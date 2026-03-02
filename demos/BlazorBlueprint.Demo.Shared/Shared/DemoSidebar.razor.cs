using BlazorBlueprint.Demo.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Demo.Shared;

public partial class DemoSidebar : ComponentBase
{
    [Inject]
    private CollapsibleStateService StateService { get; set; } = null!;

    private bool _primitivesMenuOpen;
    private bool _componentsMenuOpen;
    private bool _chartsMenuOpen;
    private bool _iconsMenuOpen;
    private bool _guidesMenuOpen;

    private const string PrimitivesMenuKey = "sidebar-primitives-menu";
    private const string ComponentsMenuKey = "sidebar-components-menu";
    private const string ChartsMenuKey = "sidebar-charts-menu";
    private const string IconsMenuKey = "sidebar-icons-menu";
    private const string GuidesMenuKey = "sidebar-guides-menu";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _primitivesMenuOpen = await StateService.GetStateAsync(PrimitivesMenuKey, defaultValue: false);
            _componentsMenuOpen = await StateService.GetStateAsync(ComponentsMenuKey, defaultValue: false);
            _chartsMenuOpen = await StateService.GetStateAsync(ChartsMenuKey, defaultValue: false);
            _iconsMenuOpen = await StateService.GetStateAsync(IconsMenuKey, defaultValue: false);
            _guidesMenuOpen = await StateService.GetStateAsync(GuidesMenuKey, defaultValue: false);

            StateHasChanged();
        }
    }

    private void OnPrimitivesMenuOpenChanged(bool isOpen)
    {
        _primitivesMenuOpen = isOpen;
        StateService.SetState(PrimitivesMenuKey, isOpen);
    }

    private void OnComponentsMenuOpenChanged(bool isOpen)
    {
        _componentsMenuOpen = isOpen;
        StateService.SetState(ComponentsMenuKey, isOpen);
    }

    private void OnChartsMenuOpenChanged(bool isOpen)
    {
        _chartsMenuOpen = isOpen;
        StateService.SetState(ChartsMenuKey, isOpen);
    }

    private void OnIconsMenuOpenChanged(bool isOpen)
    {
        _iconsMenuOpen = isOpen;
        StateService.SetState(IconsMenuKey, isOpen);
    }

    private void OnGuidesMenuOpenChanged(bool isOpen)
    {
        _guidesMenuOpen = isOpen;
        StateService.SetState(GuidesMenuKey, isOpen);
    }
}
