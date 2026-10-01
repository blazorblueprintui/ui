namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Tracks an overlay's closed-state exit animation so its content stays mounted long enough to
/// animate out before being removed from the DOM.
/// <para>
/// Blazor removes an element on the same render that flips it closed, which leaves the
/// <c>data-state="closed"</c> animate-out classes no window to run in. An owner holds a gate,
/// calls <see cref="Begin"/> when closing, and renders while <see cref="IsPresent"/> is set. Once
/// the animation has played, <see cref="CompleteClose"/> reports whether the owner should now
/// unmount.
/// </para>
/// </summary>
/// <example>
/// <code>
/// private readonly ExitAnimationGate exit = new();
///
/// // Closing.
/// exit.Begin();
///
/// // Markup.
/// @if (IsOpen || exit.IsAnimatingOut) { ... }
///
/// // After the exit animation has played.
/// if (exit.CompleteClose(isOpen)) { StateHasChanged(); }
/// </code>
/// </example>
public sealed class ExitAnimationGate
{
    /// <summary>
    /// Gets whether the exit animation has been started and not yet completed. While set, the
    /// owner should keep its content mounted with <c>data-state="closed"</c>.
    /// </summary>
    public bool IsAnimatingOut { get; private set; }

    /// <summary>
    /// Starts the exit animation, keeping the content mounted. Has no effect when already started,
    /// so a repeated close cannot restart the wait.
    /// </summary>
    public void Begin() =>
        IsAnimatingOut = true;

    /// <summary>
    /// Abandons an in-flight exit animation because the overlay reopened. The content stays mounted
    /// and the enter animation takes over from the exit.
    /// </summary>
    public void Cancel() =>
        IsAnimatingOut = false;

    /// <summary>
    /// Gets whether the content should be present in the DOM: open, or playing its exit animation.
    /// </summary>
    /// <param name="isOpen">Whether the owner is currently open.</param>
    public bool IsPresent(bool isOpen) =>
        isOpen || IsAnimatingOut;

    /// <summary>
    /// Ends the exit animation, allowing the content to unmount.
    /// </summary>
    /// <param name="isOpen">
    /// Whether the owner is currently open. A reopen that beats the animation to the punch leaves
    /// the content mounted.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when this call ended an in-flight exit animation and the caller should
    /// re-render to unmount; <see langword="false"/> when there was nothing to end.
    /// </returns>
    public bool CompleteClose(bool isOpen)
    {
        if (!IsAnimatingOut || isOpen)
        {
            return false;
        }

        IsAnimatingOut = false;
        return true;
    }
}