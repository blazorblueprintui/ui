using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrimitiveSheetContent = BlazorBlueprint.Primitives.Sheet.BbSheetContent;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// <c>ShowOverlay="false"</c> on <see cref="BbSheetContent"/> opens a non-modal sheet: no
/// overlay, <c>aria-modal="false"</c>, no focus trap, no scroll lock, and a pointer-down outside
/// closes it unless <c>CloseOnInteractOutside</c> is false (#626).
/// </summary>
public class SheetNonModalTests
{
    [Fact]
    public async Task DefaultSheetIsUnchanged()
    {
        await using var context = CreateContext(out var focus);
        var host = context.Render<BbPortalHost>();

        RenderSheet(context);

        host.WaitForElement("[data-slot='sheet-overlay']");
        var content = host.WaitForElement("[role='dialog']");
        Assert.Equal("true", content.GetAttribute("aria-modal"));

        host.WaitForAssertion(() => Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "portal.lockBodyScroll"));
        Assert.Equal(1, focus.Traps);
        Assert.Equal(0, focus.Releases);
        Assert.DoesNotContain(context.JSInterop.Invocations, call => call.Identifier == "clickOutside.onClickOutsideByIds");
    }

    [Fact]
    public async Task WithoutOverlayNoOverlayIsRenderedAndTheContentIsNotModal()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        RenderSheet(context, showOverlay: false);

        var content = host.WaitForElement("[role='dialog']");
        Assert.Equal("false", content.GetAttribute("aria-modal"));
        Assert.Empty(host.FindAll("[data-slot='sheet-overlay']"));
        Assert.Empty(host.FindAll("[id$='-overlay']"));
    }

    [Fact]
    public async Task WithoutOverlayFocusIsNotTrappedAndScrollIsNotLocked()
    {
        await using var context = CreateContext(out var focus);
        var host = context.Render<BbPortalHost>();

        // TrapFocus and LockScroll keep their defaults (true): ShowOverlay="false" overrides them.
        RenderSheet(context, showOverlay: false);
        host.WaitForElement("[role='dialog']");

        host.WaitForAssertion(() => Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "clickOutside.onClickOutsideByIds"));

        // Focus moves into the sheet through the same resolution as the trap, which is released
        // straight away rather than kept.
        Assert.Equal(1, focus.Traps);
        Assert.Equal(1, focus.Releases);
        Assert.DoesNotContain(context.JSInterop.Invocations, call => call.Identifier == "portal.lockBodyScroll");
    }

    [Fact]
    public async Task WithoutOverlayOutsideDetectionWatchesTheContentAndExcludesTheTrigger()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        RenderSheet(context, showOverlay: false);
        var content = host.WaitForElement("[role='dialog']");

        host.WaitForAssertion(() => Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "clickOutside.onClickOutsideByIds"));
        var call = context.JSInterop.Invocations.Single(c => c.Identifier == "clickOutside.onClickOutsideByIds");

        Assert.Equal(content.Id, call.Arguments[0]);
        Assert.Equal(nameof(PrimitiveSheetContent.JsOnInteractOutside), call.Arguments[2]);
        Assert.Equal(content.Id!.Replace("-content", "-trigger", StringComparison.Ordinal), call.Arguments[3]);
    }

    [Fact]
    public async Task WithoutOverlayAnOutsideInteractionClosesTheSheet()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        var sheet = RenderSheet(context, showOverlay: false);
        host.WaitForElement("[role='dialog']");

        await InteractOutside(host);

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
        Assert.False(sheet.Instance.Open);
    }

    [Fact]
    public async Task CloseOnInteractOutsideFalseKeepsTheSheetOpen()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        var sheet = RenderSheet(context, showOverlay: false, closeOnInteractOutside: false);
        host.WaitForElement("[role='dialog']");

        await InteractOutside(host);

        Assert.True(sheet.Instance.Open);
        Assert.NotEmpty(host.FindAll("[role='dialog']"));
    }

    [Fact]
    public async Task ModalFalseStillPreventsAnOutsideInteractionFromClosingTheSheet()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        // Modal on BbSheet keeps its meaning: whether the sheet can be dismissed at all.
        var sheet = RenderSheet(context, showOverlay: false, modal: false);
        host.WaitForElement("[role='dialog']");

        await InteractOutside(host);
        await host.InvokeAsync(() => host.FindComponent<PrimitiveSheetContent>().Instance.JsOnEscapeKey());

        Assert.True(sheet.Instance.Open);
    }

    [Fact]
    public async Task WithoutOverlayEscapeStillClosesTheSheet()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        var sheet = RenderSheet(context, showOverlay: false);
        host.WaitForElement("[role='dialog']");
        host.WaitForAssertion(() => Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "escapeKeydown.initialize"));

        await host.InvokeAsync(() => host.FindComponent<PrimitiveSheetContent>().Instance.JsOnEscapeKey());

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
        Assert.False(sheet.Instance.Open);
    }

    [Fact]
    public async Task ClosingANonModalSheetReleasesTheOutsideListener()
    {
        await using var context = CreateContext(out _);
        var host = context.Render<BbPortalHost>();

        // Hand back a real reference so the release is observable.
        var handle = context.JSInterop.SetupModule(invocation => invocation.Identifier == "clickOutside.onClickOutsideByIds");
        handle.SetupVoid("dispose").SetVoidResult();

        RenderSheet(context, showOverlay: false);
        host.WaitForElement("[role='dialog']");
        host.WaitForAssertion(() => Assert.Contains(context.JSInterop.Invocations, call => call.Identifier == "clickOutside.onClickOutsideByIds"));

        await InteractOutside(host);

        host.WaitForAssertion(() => Assert.Contains(handle.Invocations, call => call.Identifier == "dispose"));
    }

    private static Task InteractOutside(IRenderedComponent<BbPortalHost> host) =>
        host.InvokeAsync(() => host.FindComponent<PrimitiveSheetContent>().Instance.JsOnInteractOutside());

    private static IRenderedComponent<SheetHarness> RenderSheet(
        ComponentBunitContext context,
        bool showOverlay = true,
        bool closeOnInteractOutside = true,
        bool modal = true) =>
        context.Render<SheetHarness>(parameters => parameters
            .Add(p => p.ShowOverlay, showOverlay)
            .Add(p => p.CloseOnInteractOutside, closeOnInteractOutside)
            .Add(p => p.Modal, modal));

    private static ComponentBunitContext CreateContext(out FakeFocusManager focus)
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        focus = new FakeFocusManager();
        context.Services.AddSingleton<IFocusManager>(focus);
        return context;
    }

    /// <summary>A controlled sheet that starts open, so the test can read its open state.</summary>
    private sealed class SheetHarness : ComponentBase
    {
        [Parameter] public bool ShowOverlay { get; set; } = true;

        [Parameter] public bool CloseOnInteractOutside { get; set; } = true;

        [Parameter] public bool Modal { get; set; } = true;

        public bool Open { get; private set; } = true;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<BbSheet>(0);
            builder.AddAttribute(1, nameof(BbSheet.Open), (bool?)Open);
            builder.AddAttribute(2, nameof(BbSheet.OpenChanged), EventCallback.Factory.Create<bool>(this, value => Open = value));
            builder.AddAttribute(3, nameof(BbSheet.Modal), Modal);
            builder.AddAttribute(4, nameof(BbSheet.ChildContent), (RenderFragment)(sheet =>
            {
                sheet.OpenComponent<BbSheetContent>(0);
                sheet.AddAttribute(1, nameof(BbSheetContent.ShowOverlay), ShowOverlay);
                sheet.AddAttribute(2, nameof(BbSheetContent.CloseOnInteractOutside), CloseOnInteractOutside);
                sheet.AddAttribute(3, nameof(BbSheetContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
                sheet.CloseComponent();
            }));
            builder.CloseComponent();
        }
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
}
