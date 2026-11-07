using BlazorBlueprint.Components;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A dismissing toast keeps its place, with <c>data-state="closed"</c>, until its exit animation
/// ends (#584). The provider removes it only then, a repeat dismissal does not cut the animation
/// short, and the auto-dismiss timer takes the same path as the close button.
/// </summary>
public class ToastExitLifetimeTests
{
    [Fact]
    public async Task ADismissedToastStaysMountedUntilItsExitEnds()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var provider = context.Render<BbToastProvider>();
        var toasts = context.Services.GetRequiredService<ToastService>();

        var toast = new ToastData { Description = "Saved", Duration = 0 };
        await provider.InvokeAsync(() => toasts.Show(toast));

        var element = provider.FindComponent<BbToast>().Find("div[data-state]");
        Assert.Equal("open", element.GetAttribute("data-state"));

        provider.FindComponent<BbToast>().Find("button").Click();

        provider.WaitForAssertion(() =>
            Assert.Equal("closed", provider.FindComponent<BbToast>().Find("div[data-state]").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        Assert.Contains(toasts.Toasts, t => t.Id == toast.Id);

        js.Release();

        provider.WaitForAssertion(() => Assert.DoesNotContain(toasts.Toasts, t => t.Id == toast.Id));
        provider.WaitForAssertion(() => Assert.Empty(provider.FindAll("div[data-state]")));
    }

    [Fact]
    public async Task RepeatedDismissRequestsStartOnlyOneExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var provider = context.Render<BbToastProvider>();
        var toasts = context.Services.GetRequiredService<ToastService>();

        await provider.InvokeAsync(() => toasts.Show(new ToastData { Description = "Saved", Duration = 0 }));
        provider.WaitForAssertion(() => Assert.Single(provider.FindComponents<BbToast>()));

        provider.FindComponent<BbToast>().Find("button").Click();
        provider.FindComponent<BbToast>().Find("button").Click();

        provider.WaitForAssertion(() =>
            Assert.Equal("closed", provider.FindComponent<BbToast>().Find("div[data-state]").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        provider.WaitForAssertion(() => Assert.Empty(provider.FindComponents<BbToast>()));
    }

    [Fact]
    public async Task AnAutoDismissedToastAlsoWaitsForItsExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js);
        var provider = context.Render<BbToastProvider>();
        var toasts = context.Services.GetRequiredService<ToastService>();

        await provider.InvokeAsync(() => toasts.Show(new ToastData { Description = "Saved", Duration = 200 }));
        provider.WaitForElement("div[data-state]");

        provider.WaitForAssertion(
            () => Assert.Equal("closed", provider.FindComponent<BbToast>().Find("div[data-state]").GetAttribute("data-state")),
            TimeSpan.FromSeconds(5));

        Assert.Single(provider.FindComponents<BbToast>());

        js.Release();

        provider.WaitForAssertion(() => Assert.Empty(provider.FindComponents<BbToast>()));
    }

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
