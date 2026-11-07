namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Tracks an overlay's closed-state exit animation, so its content stays on the page long enough
/// to animate out before being removed from the DOM.
/// <para>
/// Blazor removes an element on the same render that flips it closed, which leaves the
/// <c>data-state="closed"</c> animate-out classes no window to run in. An owner holds a gate,
/// registers the content's <see cref="ExitAnimation"/>, calls <see cref="Begin"/> when closing,
/// and renders while <see cref="IsPresent"/> is set. The gate runs the animation, and clears
/// presence (through the completion callback) once it has finished.
/// </para>
/// <para>
/// The gate is the single exit lifecycle shared by Dialog (through <c>DialogContext</c>), Sheet,
/// Drawer, ContextMenu and NavigationMenu. It has to be composition rather than a base class:
/// some owners are contexts and some are components, so there is no common ancestor to derive
/// from, and Components cannot see Primitives' internals.
/// </para>
/// </summary>
/// <example>
/// <code>
/// private readonly ExitAnimationGate exit = new();
///
/// // Registered by the content component that renders the animated element.
/// exit.ExitAnimation = isCurrent => OverlayExit.WaitAsync(js, element, OverlayExit.FadeFallbackDelay);
///
/// // Closing.
/// exit.Begin(NotifyStateChanged);
///
/// // Markup.
/// @if (exit.IsPresent(IsOpen)) { ... }
///
/// // Opening again before the animation ends.
/// exit.Cancel();
/// </code>
/// </example>
public sealed class ExitAnimationGate
{
    private int generation;

    /// <summary>
    /// Gets or sets the exit animation registered by the content that renders the animated
    /// element, or <see langword="null"/> when the overlay leaves the page as soon as it closes.
    /// The delegate's argument says whether this exit is still the current one: a reopen, or a
    /// newer close, supersedes it.
    /// </summary>
    public Func<Func<bool>, Task>? ExitAnimation { get; set; }

    /// <summary>
    /// Gets whether an exit animation has been started and has not finished yet. While set, the
    /// owner should keep its content mounted with <c>data-state="closed"</c>.
    /// </summary>
    public bool IsAnimatingOut { get; private set; }

    /// <summary>
    /// Gets whether the content should be present in the DOM: open, or playing its exit animation.
    /// </summary>
    /// <param name="isOpen">Whether the owner is currently open.</param>
    public bool IsPresent(bool isOpen) =>
        isOpen || IsAnimatingOut;

    /// <summary>
    /// Starts the registered exit animation and keeps the content mounted until it ends.
    /// </summary>
    /// <param name="onCompleted">
    /// Called once the animation has finished, so the owner can re-render and unmount. Not called
    /// for an exit that was superseded by a reopen or a newer close.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when an exit animation was started; <see langword="false"/> when
    /// there is nothing to play (no animation registered) or one is already in flight.
    /// </returns>
    public bool Begin(Action onCompleted)
    {
        if (IsAnimatingOut || ExitAnimation is not { } animation)
        {
            return false;
        }

        IsAnimatingOut = true;
        var token = ++generation;
        _ = FinishAsync(animation, token, onCompleted);
        return true;
    }

    /// <summary>
    /// Abandons an in-flight exit animation because the overlay reopened. The content stays mounted
    /// and the enter animation takes over from the exit.
    /// </summary>
    public void Cancel()
    {
        generation++;
        IsAnimatingOut = false;
    }

    private async Task FinishAsync(Func<Func<bool>, Task> animation, int token, Action onCompleted)
    {
        try
        {
            await animation(() => token == generation);
        }
        finally
        {
            // A reopen, or a newer close, owns presence now. An exit that ends at once does so
            // before the open state turns closed, so presence must clear here regardless of the
            // owner's current open state.
            if (token == generation)
            {
                IsAnimatingOut = false;
                onCompleted();
            }
        }
    }
}
