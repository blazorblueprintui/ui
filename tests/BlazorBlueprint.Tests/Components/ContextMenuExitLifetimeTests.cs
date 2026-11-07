using BlazorBlueprint.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using PrimitiveContextMenu = BlazorBlueprint.Primitives.ContextMenu.BbContextMenu;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A closing context menu keeps its content mounted, with <c>data-state="closed"</c>, until its
/// exit animation ends (#584); a reopen supersedes the exit and a repeat close starts no second
/// one.
/// </summary>
public class ContextMenuExitLifetimeTests
{
    [Fact]
    public async Task AClosingMenuStaysMountedUntilItsExitEnds()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        await menu.InvokeAsync(async () => await menu.Instance.OpenAt(10, 10));
        var content = menu.WaitForElement("[role='menu']");
        Assert.Equal("open", content.GetAttribute("data-state"));

        await menu.InvokeAsync(async () => await menu.Instance.Close());

        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        menu.WaitForAssertion(() => Assert.Empty(menu.FindAll("[role='menu']")));
    }

    [Fact]
    public async Task AMenuReopenedMidExitStaysOnThePageOpen()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        await menu.InvokeAsync(async () => await menu.Instance.OpenAt(10, 10));
        menu.WaitForElement("[role='menu']");

        await menu.InvokeAsync(async () => await menu.Instance.Close());
        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));

        await menu.InvokeAsync(async () => await menu.Instance.OpenAt(20, 20));

        js.Release();

        menu.WaitForAssertion(() =>
            Assert.Equal("open", menu.Find("[role='menu']").GetAttribute("data-state")));
    }

    [Fact]
    public async Task RepeatedCloseRequestsStartOnlyOneExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        await menu.InvokeAsync(async () => await menu.Instance.OpenAt(10, 10));
        menu.WaitForElement("[role='menu']");

        await menu.InvokeAsync(async () => await menu.Instance.Close());
        await menu.InvokeAsync(async () => await menu.Instance.Close());

        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);
    }

    [Fact]
    public async Task AControlledCloseAlsoWaitsForItsExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = context.Render<PrimitiveContextMenu>(parameters => parameters
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, (bool _) => { })
            .Add(p => p.ChildContent, MenuContent()));

        menu.WaitForElement("[role='menu']");

        menu.Render(parameters => parameters.Add(p => p.Open, false));

        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        menu.WaitForAssertion(() => Assert.Empty(menu.FindAll("[role='menu']")));
    }

    private static IRenderedComponent<PrimitiveContextMenu> RenderMenu(ComponentBunitContext context) =>
        context.Render<PrimitiveContextMenu>(parameters =>
            parameters.Add(p => p.ChildContent, MenuContent()));

    private static RenderFragment MenuContent() => builder =>
    {
        builder.OpenComponent<BbContextMenuContent>(0);
        builder.AddAttribute(1, nameof(BbContextMenuContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
        builder.CloseComponent();
    };

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
}
