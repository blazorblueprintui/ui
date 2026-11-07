using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// Each modal overlay carries a <c>data-slot</c>, so it can be styled without relying on its
/// generated id or the portal wrapper (#626).
/// </summary>
public class OverlaySlotTests
{
    [Fact]
    public async Task SheetOverlayHasASlot()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        context.Render<BbSheet>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .AddChildContent<BbSheetContent>(content => content.AddChildContent("Body")));

        host.WaitForElement("[data-slot='sheet-overlay']");
    }

    [Fact]
    public async Task DialogOverlayHasASlot()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        context.Render<BbDialog>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .AddChildContent<BbDialogContent>(content => content.AddChildContent("Body")));

        host.WaitForElement("[data-slot='dialog-overlay']");
    }

    [Fact]
    public async Task ServiceDialogOverlayHasASlot()
    {
        await using var context = CreateContext();
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        // OpenAsync completes when the dialog closes, so start it without waiting for it.
        _ = provider.InvokeAsync(() => _ = service.OpenAsync<Body>(new DialogOpenOptions()));

        provider.WaitForElement("[data-slot='dialog-overlay']");
    }

    [Fact]
    public async Task AlertDialogOverlayHasASlot()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();

        context.Render<BbAlertDialog>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .AddChildContent<BbAlertDialogContent>(content => content.AddChildContent("Body")));

        host.WaitForElement("[data-slot='alert-dialog-overlay']");
    }

    [Fact]
    public async Task DrawerOverlayHasASlot()
    {
        await using var context = CreateContext();

        // The drawer renders in place, not through the portal host.
        var drawer = context.Render<BbDrawer>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .AddChildContent<BbDrawerContent>(content => content.AddChildContent("Body")));

        drawer.WaitForElement("[data-slot='drawer-overlay']");
    }

    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private sealed class Body : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "Body");
    }
}
