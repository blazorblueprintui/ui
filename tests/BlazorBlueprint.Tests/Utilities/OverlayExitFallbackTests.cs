using System.Diagnostics;
using BlazorBlueprint.Primitives.Services;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Tests.Utilities;

/// <summary>
/// An overlay that waits on JS interop must still leave the page when interop is unavailable —
/// during prerendering, after a circuit drops, or when the browser cannot run the animation.
/// <see cref="OverlayExit.WaitAsync"/> falls back to a fixed delay so the overlay never hangs with
/// its content mounted and its scrim still intercepting input.
/// </summary>
public class OverlayExitFallbackTests
{
    [Fact]
    public async Task AFailedModuleImportFallsBackToTheFadeDelay()
    {
        var js = new ThrowingJavaScript(new JSDisconnectedException("circuit gone"));

        var elapsed = await TimeAsync(() => OverlayExit.WaitAsync(js, default, OverlayExit.FadeFallbackDelay));

        Assert.InRange(elapsed, TimeSpan.FromMilliseconds(150), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AFailedWaitFallsBackToTheSlideDelay()
    {
        var js = new FailingModuleJavaScript(new JSException("animation failed"));

        var elapsed = await TimeAsync(() => OverlayExit.WaitAsync(js, default, OverlayExit.SlideFallbackDelay));

        Assert.InRange(elapsed, TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task APrerenderingFailureFallsBackInsteadOfHanging()
    {
        var js = new ThrowingJavaScript(new InvalidOperationException("JS interop is not available during prerendering."));

        var elapsed = await TimeAsync(() => OverlayExit.WaitAsync(js, default, OverlayExit.FadeFallbackDelay));

        Assert.InRange(elapsed, TimeSpan.FromMilliseconds(150), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ADisposedWaitFallsBackInsteadOfThrowing()
    {
        var js = new ThrowingJavaScript(new ObjectDisposedException("module"));

        var elapsed = await TimeAsync(() => OverlayExit.WaitAsync(js, default, OverlayExit.FadeFallbackDelay));

        Assert.InRange(elapsed, TimeSpan.FromMilliseconds(150), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task AWaitThatSucceedsDoesNotPayTheFallbackDelay()
    {
        var js = new ImmediateJavaScript();

        var elapsed = await TimeAsync(() => OverlayExit.WaitAsync(js, default, OverlayExit.SlideFallbackDelay));

        Assert.True(elapsed < TimeSpan.FromMilliseconds(250), $"Waited {elapsed} for an exit that ended at once.");
    }

    private static async Task<TimeSpan> TimeAsync(Func<Task> action)
    {
        var watch = Stopwatch.StartNew();
        await action();
        return watch.Elapsed;
    }

    /// <summary>Fails the module import itself, as a prerendering or disconnected circuit does.</summary>
    private sealed class ThrowingJavaScript(Exception exception) : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromException<TValue>(exception);
    }

    /// <summary>Imports a module whose animation call fails, as a missing or broken export does.</summary>
    private sealed class FailingModuleJavaScript(Exception exception) : IJSRuntime, IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            typeof(TValue) == typeof(IJSObjectReference)
                ? ValueTask.FromResult((TValue)(object)this)
                : ValueTask.FromException<TValue>(exception);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>Imports a module whose exit wait completes at once, as no animation would.</summary>
    private sealed class ImmediateJavaScript : IJSRuntime, IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromResult(typeof(TValue) == typeof(IJSObjectReference) ? (TValue)(object)this : default!);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
