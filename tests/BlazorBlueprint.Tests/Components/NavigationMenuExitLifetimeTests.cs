using BlazorBlueprint.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A closing navigation menu dropdown keeps its content mounted, with
/// <c>data-state="closed"</c>, until its exit animation ends (#584). Closing through the parent
/// menu — the shared hover timer or navigation setting its active item to null — is detected at
/// the item, so it animates out the same way Escape does.
/// </summary>
public class NavigationMenuExitLifetimeTests
{
    [Fact]
    public async Task AClosingDropdownStaysMountedUntilItsExitEnds()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        var trigger = menu.Find("button");
        trigger.Click();
        menu.WaitForElement("[role='menu']");
        Assert.Equal("open", menu.Find("[role='menu']").GetAttribute("data-state"));

        await Escape(menu);

        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        menu.WaitForAssertion(() => Assert.Empty(menu.FindAll("[role='menu']")));
    }

    [Fact]
    public async Task ADropdownReopenedMidExitStaysOnThePageOpen()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        menu.Find("button").Click();
        menu.WaitForElement("[role='menu']");

        await Escape(menu);
        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));

        menu.Find("button").Click();

        js.Release();

        menu.WaitForAssertion(() =>
            Assert.Equal("open", menu.Find("[role='menu']").GetAttribute("data-state")));
    }

    [Fact]
    public async Task RepeatedEscapeRequestsStartOnlyOneExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var menu = RenderMenu(context);

        menu.Find("button").Click();
        menu.WaitForElement("[role='menu']");

        await Escape(menu);
        await Escape(menu);

        menu.WaitForAssertion(() =>
            Assert.Equal("closed", menu.Find("[role='menu']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);
    }

    private static Task Escape(IRenderedComponent<BbNavigationMenu> menu) =>
        menu.Find("button").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

    private static IRenderedComponent<BbNavigationMenu> RenderMenu(ComponentBunitContext context) =>
        context.Render<BbNavigationMenu>(parameters =>
            parameters.Add(p => p.ChildContent, MenuContent()));

    private static RenderFragment MenuContent() => builder =>
    {
        builder.OpenComponent<BbNavigationMenuList>(0);
        builder.AddAttribute(1, nameof(BbNavigationMenuList.ChildContent), (RenderFragment)(list =>
        {
            list.OpenComponent<BbNavigationMenuItem>(0);
            list.AddAttribute(1, nameof(BbNavigationMenuItem.Value), "products");
            list.AddAttribute(2, nameof(BbNavigationMenuItem.ChildContent), (RenderFragment)(item =>
            {
                item.OpenComponent<BbNavigationMenuTrigger>(0);
                item.AddAttribute(1, nameof(BbNavigationMenuTrigger.ChildContent), (RenderFragment)(label => label.AddContent(0, "Products")));
                item.CloseComponent();

                item.OpenComponent<BbNavigationMenuContent>(2);
                item.AddAttribute(3, nameof(BbNavigationMenuContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
                item.CloseComponent();
            }));
            list.CloseComponent();
        }));
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
