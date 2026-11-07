using AngleSharp.Dom;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// <see cref="BbDialog"/> and <see cref="DialogOpenOptions"/> play a chosen Motion enter and exit,
/// and a closing dialog stays on the page until its exit ends (#613).
/// </summary>
public class DialogAnimationTests
{
    [Fact]
    public async Task ADialogWithNoAnimationLeavesAsSoonAsItCloses()
    {
        await using var context = CreateContext();
        var (host, transitions) = Render(context, exitPreset: null);

        CloseByButton(host);

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
        Assert.DoesNotContain(transitions.Invocations, call => IsExit(call));
    }

    [Fact]
    public async Task ADialogStaysOnThePageUntilItsExitEnds()
    {
        await using var context = CreateContext();
        var (host, transitions) = Render(context, exitPreset: MotionPreset.SlideRight);

        CloseByButton(host);

        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));
        var exit = Assert.Single(transitions.Invocations, call => IsExit(call));
        Assert.Equal("SlideRight", Config(exit, "preset"));
        Assert.Equal(200, Config(exit, "duration"));

        _ = host.InvokeAsync(() => transitions.SetVoidResult());

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
    }

    [Fact]
    public async Task AControlledCloseAlsoWaitsForTheExit()
    {
        await using var context = CreateContext();
        var transitions = SetupTransitions(context);
        var host = context.Render<BbPortalHost>();
        var dialog = context.Render<BbDialog>(parameters => parameters
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, (bool _) => { })
            .Add(p => p.ExitPreset, MotionPreset.Fade)
            .AddChildContent<BbDialogContent>(content => content.AddChildContent("Body")));
        host.WaitForElement("[role='dialog']");

        dialog.Render(parameters => parameters.Add(p => p.Open, false));

        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));

        _ = host.InvokeAsync(() => transitions.SetVoidResult());

        host.WaitForAssertion(() => Assert.Empty(host.FindAll("[role='dialog']")));
    }

    [Fact]
    public async Task ADialogReopenedMidExitStaysOnThePage()
    {
        await using var context = CreateContext();
        var transitions = SetupTransitions(context);
        var host = context.Render<BbPortalHost>();
        var dialog = context.Render<BbDialog>(parameters => parameters
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, (bool _) => { })
            .Add(p => p.ExitPreset, MotionPreset.Fade)
            .AddChildContent<BbDialogContent>(content => content.AddChildContent("Body")));
        host.WaitForElement("[role='dialog']");

        dialog.Render(parameters => parameters.Add(p => p.Open, false));
        host.WaitForAssertion(() =>
            Assert.Equal("closed", host.Find("[role='dialog']").GetAttribute("data-state")));
        dialog.Render(parameters => parameters.Add(p => p.Open, true));

        // The old exit ends after the reopen; it must not take the dialog away.
        _ = host.InvokeAsync(() => transitions.SetVoidResult());

        host.WaitForAssertion(() =>
            Assert.Equal("open", host.Find("[role='dialog']").GetAttribute("data-state")));
    }

    [Fact]
    public async Task AnEnterPresetPlaysWhenTheDialogOpensInPlaceOfTheBuiltInZoom()
    {
        await using var context = CreateContext();
        var (host, transitions) = Render(context, enterPreset: MotionPreset.Scale);

        var content = host.WaitForElement("[role='dialog']");

        host.WaitForAssertion(() => Assert.Contains(transitions.Invocations, call => !IsExit(call)));
        var enter = transitions.Invocations.First(call => !IsExit(call));
        Assert.Equal("Scale", Config(enter, "preset"));
        Assert.Equal(200, Config(enter, "duration"));
        Assert.DoesNotContain("bb:data-[state=open]:zoom-in-95", content.ClassList);
    }

    [Fact]
    public async Task ADialogWithNoEnterPresetKeepsTheBuiltInZoom()
    {
        await using var context = CreateContext();
        var (host, _) = Render(context, exitPreset: MotionPreset.Fade);

        var content = host.WaitForElement("[role='dialog']");

        Assert.Contains("bb:data-[state=open]:zoom-in-95", content.ClassList);
    }

    [Fact]
    public async Task AServiceDialogStaysOnThePageUntilItsExitEnds()
    {
        await using var context = CreateContext();
        var transitions = SetupTransitions(context);
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        Task<DialogResult>? result = null;
        _ = provider.InvokeAsync(() => result = service.OpenAsync<Body>(new DialogOpenOptions
        {
            EnterPreset = MotionPreset.SlideLeft,
            ExitPreset = MotionPreset.SlideLeft,
            AnimationDuration = 300
        }));
        provider.WaitForElement("[role='dialog']");
        provider.WaitForAssertion(() => Assert.Contains(transitions.Invocations, call => !IsExit(call)));
        Assert.Equal(300, Config(transitions.Invocations.First(call => !IsExit(call)), "duration"));

        _ = provider.InvokeAsync(() => service.CancelAll());

        // The caller has its answer at once; only the dialog's removal waits for the exit.
        provider.WaitForAssertion(() =>
            Assert.Equal("closed", provider.Find("[role='dialog']").GetAttribute("data-state")));
        Assert.True(result!.IsCompleted);
        Assert.Contains(transitions.Invocations, call => IsExit(call));

        _ = provider.InvokeAsync(() => transitions.SetVoidResult());

        provider.WaitForAssertion(() => Assert.Empty(provider.FindAll("[role='dialog']")));
    }

    [Fact]
    public async Task AServiceDialogWithNoAnimationLeavesAsSoonAsItCloses()
    {
        await using var context = CreateContext();
        var transitions = SetupTransitions(context);
        var provider = context.Render<BbDialogProvider>();
        var service = context.Services.GetRequiredService<DialogService>();

        _ = provider.InvokeAsync(() => _ = service.OpenAsync<Body>(new DialogOpenOptions()));
        var content = provider.WaitForElement("[role='dialog']");
        Assert.Contains("bb:zoom-in-95", content.ClassList);

        _ = provider.InvokeAsync(() => service.CancelAll());

        provider.WaitForAssertion(() => Assert.Empty(provider.FindAll("[role='dialog']")));
        Assert.Empty(transitions.Invocations);
    }

    private static (IRenderedComponent<BbPortalHost> Host, JSRuntimeInvocationHandler Transitions) Render(
        ComponentBunitContext context,
        MotionPreset? enterPreset = null,
        MotionPreset? exitPreset = null)
    {
        var transitions = SetupTransitions(context);
        var host = context.Render<BbPortalHost>();

        context.Render<BbDialog>(parameters => parameters
            .Add(p => p.DefaultOpen, true)
            .Add(p => p.EnterPreset, enterPreset)
            .Add(p => p.ExitPreset, exitPreset)
            .AddChildContent<BbDialogContent>(content => content.AddChildContent("Body")));

        host.WaitForElement("[role='dialog']");
        return (host, transitions);
    }

    /// <summary>
    /// Holds every Motion transition open until the test ends it, as a running animation would.
    /// </summary>
    private static JSRuntimeInvocationHandler SetupTransitions(ComponentBunitContext context) =>
        context.JSInterop
            .SetupModule(invocation => invocation.Arguments.Count > 0 && invocation.Arguments[0]?.ToString()?.Contains("motion.js", StringComparison.Ordinal) == true)
            .SetupVoid("transition", _ => true);

    private static void CloseByButton(IRenderedComponent<BbPortalHost> host) =>
        host.Find("[role='dialog'] button").Click();

    private static bool IsExit(JSRuntimeInvocation call) => (bool)Config(call, "exiting")!;

    private static object? Config(JSRuntimeInvocation call, string name)
    {
        var config = call.Arguments[1]!;
        return config.GetType().GetProperty(name)!.GetValue(config);
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
