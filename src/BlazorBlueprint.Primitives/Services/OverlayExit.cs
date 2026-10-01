using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Waits for an element's closed-state exit animation to finish, for overlays that keep their
/// content mounted with <c>data-state="closed"</c> until the animation has played.
/// </summary>
public static class OverlayExit
{
    /// <summary>
    /// The delay to use when JS interop is unavailable and the overlay's exit is a plain fade
    /// (menus, panels, toasts). Matches tw-animate's 150ms default plus slack.
    /// </summary>
    public static readonly TimeSpan FadeFallbackDelay = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The delay to use when JS interop is unavailable and the overlay's exit slides as well as
    /// fading (dialog, sheet and drawer panels, the 300ms off-canvas and zoom-out durations).
    /// </summary>
    public static readonly TimeSpan SlideFallbackDelay = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// Waits until <paramref name="element"/> has finished its exit animation, then returns.
    /// <para>
    /// Callers keep the element mounted while awaiting this, so the animate-out classes actually get
    /// a window to run; when it returns the caller unmounts.
    /// </para>
    /// </summary>
    /// <param name="js">The runtime to invoke the shared primitive bundle on.</param>
    /// <param name="element">The element whose exit animation to wait on.</param>
    /// <param name="fallbackDelay">
    /// How long to wait when JS interop fails. Pass <see cref="FadeFallbackDelay"/> for a plain
    /// fade-out, <see cref="SlideFallbackDelay"/> for one that slides as well.
    /// </param>
    /// <remarks>
    /// JS interop is unavailable during prerendering and after a circuit drops, and an overlay that
    /// waits on it there would hang with its content mounted and its scrim still intercepting input.
    /// Those paths fall back to a fixed delay so the overlay still closes.
    /// </remarks>
    public static async Task WaitAsync(IJSRuntime js, ElementReference element, TimeSpan fallbackDelay)
    {
        try
        {
            var module = await PrimitiveModules.GetAsync(js);
            await module.InvokeVoidAsync("animation.waitForExit", element);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            try
            {
                await Task.Delay(fallbackDelay);
            }
            catch (ObjectDisposedException)
            {
                // Component disposed while waiting.
            }
        }
    }
}