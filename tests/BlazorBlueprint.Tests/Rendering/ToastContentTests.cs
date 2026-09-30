using BlazorBlueprint.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorBlueprint.Tests.Rendering;

/// <summary>
/// A toast can hold components as well as text: a custom icon, a body and a row of actions.
/// <para>
/// Those components bring overlays of their own. A menu opened from a toast renders outside it,
/// so moving the pointer onto the menu reads as leaving the toast, and hover alone would let the
/// timer dismiss the toast, menu and all, while the user is still choosing. Pressing or focusing
/// inside such a toast holds it open instead.
/// </para>
/// </summary>
public class ToastContentTests
{
    [Fact]
    public async Task ContentAndActionsRenderAfterTheText()
    {
        var markup = await RenderToastAsync(new ToastData
        {
            Title = "Clara replied",
            Description = "ML-15 is ready for your review.",
            Content = Element("p", "reply-body"),
            Actions = Element("button", "open-chat")
        });

        var description = markup.IndexOf("ML-15 is ready", StringComparison.Ordinal);
        var body = markup.IndexOf("reply-body", StringComparison.Ordinal);
        var actions = markup.IndexOf("open-chat", StringComparison.Ordinal);

        Assert.True(description >= 0 && body > description && actions > body, markup);
    }

    [Fact]
    public async Task ACustomIconReplacesTheVariantIcon()
    {
        var variantIcon = await RenderToastAsync(new ToastData
        {
            Description = "Saved",
            Variant = ToastVariant.Success,
            ShowClose = false
        });
        var customIcon = await RenderToastAsync(new ToastData
        {
            Description = "Saved",
            Variant = ToastVariant.Success,
            ShowClose = false,
            Icon = Element("span", "avatar")
        });

        Assert.Contains("<svg", variantIcon, StringComparison.Ordinal);
        Assert.Contains("avatar", customIcon, StringComparison.Ordinal);
        Assert.DoesNotContain("<svg", customIcon, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACustomIconShowsOnTheDefaultVariantAndShowIconStillHidesIt()
    {
        var shown = await RenderToastAsync(new ToastData { Description = "Hi", Icon = Element("span", "avatar") });
        var hidden = await RenderToastAsync(new ToastData { Description = "Hi", Icon = Element("span", "avatar"), ShowIcon = false });

        Assert.Contains("avatar", shown, StringComparison.Ordinal);
        Assert.DoesNotContain("avatar", hidden, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PressingInsideAToastWithActionsHoldsItAndLeavingDoesNotResume()
    {
        var (holds, resumes) = await PressThenLeaveAsync(new ToastData
        {
            Description = "Export ready",
            Actions = Element("button", "download")
        });

        Assert.Equal(1, holds);
        Assert.Equal(0, resumes);
    }

    [Fact]
    public async Task PressingInsideAPlainToastDoesNotHoldIt()
    {
        var (holds, resumes) = await PressThenLeaveAsync(new ToastData { Description = "Saved" });

        Assert.Equal(0, holds);
        Assert.Equal(1, resumes);
    }

    /// <summary>
    /// Two toasts with the same duration, the held one shown first: once the other has timed out,
    /// the held one's timer would have fired too.
    /// </summary>
    [Fact]
    public async Task AHeldToastOutlivesItsDuration()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var toasts = provider.GetRequiredService<ToastService>();

        var held = new ToastData { Description = "Export ready", Duration = 200, Actions = Element("button", "download") };
        var plain = new ToastData { Description = "Saved", Duration = 200 };

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbToastProvider>([]);
            toasts.Show(held);
            toasts.Show(plain);

            // Render order: the held toast is the first carrying the handler.
            await renderer.DispatchAsync("onpointerdown", new PointerEventArgs());
        });

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (toasts.Toasts.Any(t => t.Id == plain.Id) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        Assert.DoesNotContain(toasts.Toasts, t => t.Id == plain.Id);
        Assert.Contains(toasts.Toasts, t => t.Id == held.Id);
    }

    [Fact]
    public async Task AComponentShownInAToastGetsItsParametersAndCanDismissTheToast()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var toasts = provider.GetRequiredService<ToastService>();

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbToastProvider>([]);
            toasts.Show<ReplyContent>(
                new() { [nameof(ReplyContent.Author)] = "Clara" },
                new ToastData { Title = "New reply", Duration = 0 });

            var markup = renderer.Markup();
            Assert.Contains("New reply", markup, StringComparison.Ordinal);
            Assert.Contains("Clara replied", markup, StringComparison.Ordinal);

            // The component's button renders before the toast's own close button.
            await renderer.DispatchAsync("onclick", new MouseEventArgs());
        });

        Assert.Empty(toasts.Toasts);
    }

    [Fact]
    public async Task ReusingTheParametersDictionaryDoesNotChangeAToastAlreadyShowing()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var toasts = provider.GetRequiredService<ToastService>();

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbToastProvider>([]);

            var parameters = new Dictionary<string, object?> { [nameof(ReplyContent.Author)] = "Clara" };
            toasts.Show<ReplyContent>(parameters, new ToastData { Duration = 0 });
            parameters[nameof(ReplyContent.Author)] = "Pip";
            toasts.Show<ReplyContent>(parameters, new ToastData { Duration = 0 });

            var markup = renderer.Markup();
            Assert.Contains("Clara replied", markup, StringComparison.Ordinal);
            Assert.Contains("Pip replied", markup, StringComparison.Ordinal);
        });
    }

    private async Task<(int Holds, int Resumes)> PressThenLeaveAsync(ToastData data)
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);
        var holds = 0;
        var resumes = 0;

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.MountAsync<BbToast>(new()
            {
                [nameof(BbToast.Data)] = data,
                [nameof(BbToast.OnHold)] = EventCallback.Factory.Create(this, () => holds++),
                [nameof(BbToast.OnResume)] = EventCallback.Factory.Create(this, () => resumes++)
            });

            await renderer.DispatchAsync("onmouseenter", new MouseEventArgs());
            await renderer.DispatchAsync("onpointerdown", new PointerEventArgs());
            await renderer.DispatchAsync("onfocusin", new FocusEventArgs());
            await renderer.DispatchAsync("onmouseleave", new MouseEventArgs());
        });

        return (holds, resumes);
    }

    private static async Task<string> RenderToastAsync(ToastData data)
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new ComponentTestRenderer(provider, NullLoggerFactory.Instance);

        await renderer.Dispatcher.InvokeAsync(async () =>
            await renderer.MountAsync<BbToast>(new() { [nameof(BbToast.Data)] = data }));

        return renderer.Markup();
    }

    private static RenderFragment Element(string name, string id) => builder =>
    {
        builder.OpenElement(0, name);
        builder.AddAttribute(1, "id", id);
        builder.CloseElement();
    };

    private sealed class ReplyContent : ComponentBase
    {
        [CascadingParameter]
        public IToastReference Toast { get; set; } = default!;

        [Parameter]
        public string Author { get; set; } = "";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Toast.DismissAsync));
            builder.AddContent(2, $"{Author} replied");
            builder.CloseElement();
        }
    }

    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IJSRuntime, NoopJavaScript>();
        services.AddBlazorBlueprintComponents();
        return services;
    }
}
