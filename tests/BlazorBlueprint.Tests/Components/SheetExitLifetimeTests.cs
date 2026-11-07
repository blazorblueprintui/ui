using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using PrimitiveSheet = BlazorBlueprint.Primitives.Sheet.BbSheet;
using PrimitiveSheetContent = BlazorBlueprint.Primitives.Sheet.BbSheetContent;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// A closing sheet keeps its overlay and content mounted, with <c>data-state="closed"</c>, until
/// its exit animation ends (#584). The focus trap and scroll lock it holds stay in place until
/// then, a reopen supersedes the exit, and a repeated close request does not start a second one.
/// </summary>
public class SheetExitLifetimeTests
{
    [Fact]
    public async Task AClosingSheetStaysMountedUntilItsExitEnds()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js, out var focus);
        var host = context.Render<BbPortalHost>();
        var sheet = RenderSheet(context, open: true);

        var content = host.WaitForElement("[role='dialog']");
        Assert.Equal("open", content.GetAttribute("data-state"));
        Assert.Equal(1, focus.Traps);

        sheet.Render(parameters => parameters.Add(p => p.Open, false));

        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));

        host.WaitForElement("[data-slot='sheet-overlay']");
        Assert.Equal(0, focus.Releases);

        js.Release();

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[data-slot='sheet-overlay']")));
        host.WaitForAssertion(() => Assert.Equal(1, focus.Releases));
    }

    [Fact]
    public async Task ASheetReopenedMidExitStaysOnThePageOpen()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js, out _);
        var host = context.Render<BbPortalHost>();
        var sheet = RenderSheet(context, open: true);
        host.WaitForElement("[role='dialog']");

        sheet.Render(parameters => parameters.Add(p => p.Open, false));
        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));

        sheet.Render(parameters => parameters.Add(p => p.Open, true));

        // The superseded exit ends after the reopen; it must not take the sheet away.
        js.Release();

        host.WaitForAssertion(() =>
            Assert.Equal("open", host.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.NotEmpty(host.FindAll("[data-slot='sheet-overlay']"));
    }

    [Fact]
    public async Task RepeatedCloseRequestsStartOnlyOneExit()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js, out _);
        var host = context.Render<BbPortalHost>();
        var sheet = RenderSheet(context, open: true);
        host.WaitForElement("[role='dialog']");

        sheet.Render(parameters => parameters.Add(p => p.Open, false));
        sheet.Render(parameters => parameters.Add(p => p.Open, false));

        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));
        // Only one exit wait was issued.
        Assert.Equal(1, js.WaitCount);
    }

    [Fact]
    public async Task AnEscapedSheetWaitsForItsExitAndIgnoresASecondEscape()
    {
        var js = new HeldExitJavaScript();
        await using var context = CreateContext(js, out _);
        var host = context.Render<BbPortalHost>();
        RenderSheet(context, open: true);
        host.WaitForElement("[role='dialog']");

        var content = host.FindComponent<PrimitiveSheetContent>();
        await content.InvokeAsync(content.Instance.JsOnEscapeKey);
        await content.InvokeAsync(content.Instance.JsOnEscapeKey);

        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.Equal(1, js.WaitCount);

        js.Release();

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
    }

    private static IRenderedComponent<SheetHarness> RenderSheet(ComponentBunitContext context, bool open) =>
        context.Render<SheetHarness>(parameters => parameters
            .Add(p => p.Open, open)
            .Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(new object(), _ => { })));

    private static ComponentBunitContext CreateContext(HeldExitJavaScript js, out FakeFocusManager focus)
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        // Replace every registered JS runtime with our held-exit fake.
        foreach (var d in context.Services.Where(d => d.ServiceType == typeof(IJSRuntime)).ToList())
        {
            context.Services.Remove(d);
        }
        context.Services.AddSingleton<IJSRuntime>(js);
        focus = new FakeFocusManager();
        context.Services.AddSingleton<IFocusManager>(focus);
        return context;
    }

    /// <summary>A controlled sheet whose open state the test can move in either direction.</summary>
    private sealed class SheetHarness : ComponentBase
    {
        [Parameter]
        public bool Open { get; set; }

        [Parameter]
        public EventCallback<bool> OpenChanged { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<PrimitiveSheet>(0);
            builder.AddAttribute(1, nameof(PrimitiveSheet.Open), (bool?)Open);
            builder.AddAttribute(2, nameof(PrimitiveSheet.OpenChanged), OpenChanged);
            builder.AddAttribute(3, nameof(PrimitiveSheet.ChildContent), (RenderFragment)(sheet =>
            {
                sheet.OpenComponent<BbSheetContent>(0);
                sheet.AddAttribute(1, nameof(BbSheetContent.ChildContent), (RenderFragment)(body => body.AddContent(0, "Body")));
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

