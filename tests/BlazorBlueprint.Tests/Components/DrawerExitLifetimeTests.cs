using BlazorBlueprint.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A closing drawer keeps its overlay and content mounted, with <c>data-state="closed"</c>, until
/// its exit animation ends (#584); a reopen supersedes the exit and a repeat close starts no
/// second one.
/// </summary>
public class DrawerExitLifetimeTests
{
    [Fact]
    public async Task AClosingDrawerStaysMountedUntilItsExitEnds()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var waits = js;
        var drawer = RenderDrawer(context, open: true);

        var content = drawer.WaitForElement("[role='dialog']");
        Assert.Equal("open", content.GetAttribute("data-state"));
        var overlay = drawer.WaitForElement("[data-slot='drawer-overlay']");
        Assert.Equal("open", overlay.GetAttribute("data-state"));

        drawer.Render(parameters => parameters.Add(p => p.Open, false));

        drawer.WaitForAssertion(() =>
            Assert.Equal("closed", drawer.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.Equal("closed", drawer.Find("[data-slot='drawer-overlay']").GetAttribute("data-state"));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        drawer.WaitForAssertion(() => Assert.Empty(drawer.FindAll("[role='dialog']")));
        drawer.WaitForAssertion(() => Assert.Empty(drawer.FindAll("[data-slot='drawer-overlay']")));
    }

    [Fact]
    public async Task ADrawerReopenedMidExitStaysOnThePageOpen()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var drawer = RenderDrawer(context, open: true);
        drawer.WaitForElement("[role='dialog']");

        drawer.Render(parameters => parameters.Add(p => p.Open, false));
        drawer.WaitForAssertion(() =>
            Assert.Equal("closed", drawer.Find("[role='dialog']").GetAttribute("data-state")));

        drawer.Render(parameters => parameters.Add(p => p.Open, true));

        js.Release();

        drawer.WaitForAssertion(() =>
            Assert.Equal("open", drawer.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.Equal("open", drawer.Find("[data-slot='drawer-overlay']").GetAttribute("data-state"));
    }

    [Fact]
    public async Task RepeatedCloseRequestsStartOnlyOneExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var drawer = RenderDrawer(context, open: true);
        drawer.WaitForElement("[role='dialog']");

        drawer.Render(parameters => parameters.Add(p => p.Open, false));
        drawer.Render(parameters => parameters.Add(p => p.Open, false));

        drawer.WaitForAssertion(() =>
            Assert.Equal("closed", drawer.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);
    }

    private static IRenderedComponent<DrawerHarness> RenderDrawer(ComponentBunitContext context, bool open) =>
        context.Render<DrawerHarness>(parameters => parameters.Add(p => p.Open, open));

    private static ComponentBunitContext CreateContext(HeldExitJavaScript js)
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        foreach (var d in context.Services.Where(d => d.ServiceType == typeof(IJSRuntime)).ToList())
        {
            context.Services.Remove(d);
        }
        context.Services.AddSingleton<IJSRuntime>(js);
        return context;
    }

    /// <summary>A controlled drawer whose open state the test can move in either direction.</summary>
    private sealed class DrawerHarness : ComponentBase
    {
        [Parameter]
        public bool Open { get; set; }

        [Parameter]
        public EventCallback<bool> OpenChanged { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<BbDrawer>(0);
            builder.AddAttribute(1, nameof(BbDrawer.Open), (bool?)Open);
            builder.AddAttribute(2, nameof(BbDrawer.OpenChanged), OpenChanged);
            builder.AddAttribute(3, nameof(BbDrawer.ChildContent), (RenderFragment)(drawer =>
            {
                drawer.OpenComponent<BbDrawerContent>(0);
                drawer.AddAttribute(1, nameof(BbDrawerContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
                drawer.CloseComponent();
            }));
            builder.CloseComponent();
        }
    }
}
