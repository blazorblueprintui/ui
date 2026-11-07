using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using PrimitiveDialog = BlazorBlueprint.Primitives.Dialog.BbDialog;
using PrimitiveDialogContent = BlazorBlueprint.Primitives.Dialog.BbDialogContent;
using PrimitiveDialogPortal = BlazorBlueprint.Primitives.Dialog.BbDialogPortal;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// How long a closing dialog keeps its place, and its modal protection, while its exit plays (#613).
/// </summary>
public class DialogExitLifetimeTests
{
    [Fact]
    public async Task AnExitThatEndsAtOnceReleasesAControlledClose()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();
        var dialog = context.Render<PrimitiveDialog>(parameters => parameters
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, (bool _) => { })
            .Add(p => p.ChildContent, PrimitiveContent()));
        host.WaitForElement("[role='dialog']");

        dialog.Render(parameters => parameters.Add(p => p.Open, false));

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
    }

    [Fact]
    public async Task AnExitThatEndsAtOnceReleasesAContextClose()
    {
        await using var context = CreateContext();
        var host = context.Render<BbPortalHost>();
        context.Render<PrimitiveDialog>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .Add(p => p.ChildContent, PrimitiveContent()));
        host.WaitForElement("[role='dialog']");

        var content = host.FindComponent<PrimitiveDialogContent>();
        await content.InvokeAsync(content.Instance.JsOnEscapeKey);

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
    }

    [Fact]
    public async Task ANativeDialogReopenedMidExitIsNotClosedByTheOldExit()
    {
        await using var context = CreateContext();
        var native = new FakeNativeOverlay();
        context.Services.AddSingleton<INativeOverlayService>(native);
        var transitions = SetupTransitions(context);
        var dialog = RenderNativeDialog(context);

        dialog.Render(parameters => parameters.Add(p => p.Open, false));
        dialog.WaitForAssertion(() => Assert.Equal("closed", dialog.Find("dialog").GetAttribute("data-state")));
        dialog.Render(parameters => parameters.Add(p => p.Open, true));

        _ = dialog.InvokeAsync(() => transitions.SetVoidResult());

        dialog.WaitForAssertion(() => Assert.Equal("open", dialog.Find("dialog").GetAttribute("data-state")));
        Assert.Equal(0, native.Closes);
    }

    [Fact]
    public async Task ASupersededNativeExitLeavesTheCloseToTheCurrentOne()
    {
        await using var context = CreateContext();
        var native = new FakeNativeOverlay();
        context.Services.AddSingleton<INativeOverlayService>(native);
        var transitions = SetupTransitions(context);
        var dialog = RenderNativeDialog(context);

        dialog.Render(parameters => parameters.Add(p => p.Open, false));
        dialog.WaitForAssertion(() => Assert.Equal("closed", dialog.Find("dialog").GetAttribute("data-state")));
        dialog.Render(parameters => parameters.Add(p => p.Open, true));
        dialog.WaitForAssertion(() => Assert.Equal("open", dialog.Find("dialog").GetAttribute("data-state")));
        dialog.Render(parameters => parameters.Add(p => p.Open, false));

        _ = dialog.InvokeAsync(() => transitions.SetVoidResult());

        dialog.WaitForAssertion(() => Assert.Empty(dialog.FindAll("dialog")));
        // The cleanup after removal may close it once more; the superseded exit must not add one.
        Assert.Equal(1, native.ClosesBeforeRemoval);
    }

    [Fact]
    public async Task TheLastServiceDialogKeepsItsModalProtectionUntilItsExitEnds()
    {
        await using var context = CreateContext();
        var focus = new FakeFocusManager();
        context.Services.AddSingleton<IFocusManager>(focus);
        var transitions = SetupTransitions(context);
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        Task<DialogResult>? result = null;
        _ = provider.InvokeAsync(() => result = service.OpenAsync<Body>(Animated()));
        provider.WaitForAssertion(() => Assert.Equal(1, focus.Traps));

        _ = provider.InvokeAsync(() => service.CancelAll());
        provider.WaitForAssertion(() =>
            Assert.Equal("closed", provider.Find("[role='dialog']").GetAttribute("data-state")));

        Assert.True(result!.IsCompleted);
        Assert.Equal(0, focus.Releases);
        Assert.DoesNotContain(context.JSInterop.Invocations, call => call.Identifier == "escapeKeydown.dispose");

        _ = provider.InvokeAsync(() => transitions.SetVoidResult());

        provider.WaitForAssertion(() => Assert.Empty(provider.FindAll("[role='dialog']")));
        provider.WaitForAssertion(() => Assert.Equal(1, focus.Releases));
        Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "escapeKeydown.dispose");
    }

    [Fact]
    public async Task AStackedServiceDialogKeepsTheFocusTrapUntilItsExitEnds()
    {
        await using var context = CreateContext();
        var focus = new FakeFocusManager();
        context.Services.AddSingleton<IFocusManager>(focus);
        var transitions = SetupTransitions(context);
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        _ = provider.InvokeAsync(() => service.OpenAsync<Body>(new DialogOpenOptions()));
        provider.WaitForAssertion(() => Assert.Equal(1, focus.Traps));
        _ = provider.InvokeAsync(() => service.OpenAsync<Body>(Animated()));
        provider.WaitForAssertion(() => Assert.Equal(2, focus.Traps));

        var top = service.Dialogs[^1].Id;
        _ = provider.InvokeAsync(() => service.Resolve(top, DialogResult.Cancel()));
        provider.WaitForAssertion(() =>
            Assert.Equal("closed", provider.FindAll("[role='dialog']")[^1].GetAttribute("data-state")));

        // Focus stays in the exiting dialog, which is still on top, not the one underneath.
        Assert.Equal(2, focus.Traps);
        Assert.Equal(1, focus.Releases);

        _ = provider.InvokeAsync(() => transitions.SetVoidResult());

        provider.WaitForAssertion(() => Assert.Single(provider.FindAll("[role='dialog']")));
        provider.WaitForAssertion(() => Assert.Equal(3, focus.Traps));
        Assert.Equal(2, focus.Releases);
    }

    private static RenderFragment PrimitiveContent() => builder =>
    {
        builder.OpenComponent<PrimitiveDialogPortal>(0);
        builder.AddAttribute(1, nameof(PrimitiveDialogPortal.ChildContent), (RenderFragment)(portal =>
        {
            portal.OpenComponent<PrimitiveDialogContent>(0);
            portal.AddAttribute(1, nameof(PrimitiveDialogContent.ExitAnimation), (Func<ElementReference, Task>)(_ => Task.CompletedTask));
            portal.AddAttribute(2, nameof(PrimitiveDialogContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
            portal.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static IRenderedComponent<BbDialog> RenderNativeDialog(ComponentBunitContext context)
    {
        var dialog = context.Render<BbDialog>(parameters => parameters
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, (bool _) => { })
            .Add(p => p.RenderingStrategy, OverlayRenderingStrategy.Native)
            .Add(p => p.ExitPreset, MotionPreset.Fade)
            .Add(p => p.AnimationDuration, 3000)
            .AddChildContent<BbDialogContent>(content => content.AddChildContent("Body")));
        dialog.WaitForAssertion(() => Assert.Equal("open", dialog.Find("dialog").GetAttribute("data-state")));
        return dialog;
    }

    private static DialogOpenOptions Animated() => new()
    {
        ExitPreset = MotionPreset.Fade,
        AnimationDuration = 3000
    };

    /// <summary>
    /// Holds every Motion transition open until the test ends it, as a running animation would.
    /// </summary>
    private static JSRuntimeInvocationHandler SetupTransitions(ComponentBunitContext context) =>
        context.JSInterop
            .SetupModule(invocation => invocation.Arguments.Count > 0 && invocation.Arguments[0]?.ToString()?.Contains("motion.js", StringComparison.Ordinal) == true)
            .SetupVoid("transition", _ => true);

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

    private sealed class FakeFocusManager : IFocusManager
    {
        public int Traps { get; private set; }

        public int Releases { get; private set; }

        public Task<IAsyncDisposable> TrapFocus(ElementReference container)
        {
            Traps++;
            return Task.FromResult<IAsyncDisposable>(new Release(this));
        }

        public Task RestoreFocus(ElementReference? previousElement) => Task.CompletedTask;

        public Task FocusFirst(ElementReference container) => Task.CompletedTask;

        public Task FocusLast(ElementReference container) => Task.CompletedTask;

        private sealed class Release(FakeFocusManager owner) : IAsyncDisposable
        {
            public ValueTask DisposeAsync()
            {
                owner.Releases++;
                return ValueTask.CompletedTask;
            }
        }
    }

    private sealed class FakeNativeOverlay : INativeOverlayService
    {
        private bool removed;

        public int Closes { get; private set; }

        public int ClosesBeforeRemoval { get; private set; }

        public Task<bool> IsDialogSupportedAsync() => Task.FromResult(true);

        public OverlayRenderingStrategy ResolveStrategy(OverlayRenderingStrategy? requested) =>
            requested ?? OverlayRenderingStrategy.JavaScript;

        public void FallBackToJavaScript() { }

        public Task ShowDialogAsync(ElementReference element) => Task.CompletedTask;

        public Task CloseDialogAsync(ElementReference element, string? returnValue = null)
        {
            Closes++;
            if (!removed)
            {
                ClosesBeforeRemoval++;
            }

            return Task.CompletedTask;
        }

        public Task FocusDialogAsync(ElementReference element) => Task.CompletedTask;

        public Task FocusTriggerAsync(ElementReference element) => Task.CompletedTask;

        public Task<IAsyncDisposable> SetupDialogAsync(ElementReference element, object dotNetRef) =>
            Task.FromResult<IAsyncDisposable>(new Setup(this));

        private sealed class Setup(FakeNativeOverlay owner) : IAsyncDisposable
        {
            // The native cleanup disposes this setup first, once the dialog has left the page.
            public ValueTask DisposeAsync()
            {
                owner.removed = true;
                return ValueTask.CompletedTask;
            }
        }
    }
}
