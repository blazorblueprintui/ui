namespace BlazorBlueprint.Primitives.Floating;

/// <summary>
/// What dismissed a floating overlay.
/// </summary>
public enum FloatingDismissReason
{
    /// <summary>The user interacted outside the content and outside its trigger.</summary>
    OutsideInteraction,

    /// <summary>The user pressed Escape.</summary>
    EscapeKey,
}

/// <summary>
/// Declares which dismissal gestures a floating overlay listens for, so that
/// <see cref="BbFloatingPortal"/> can wire them in the same interop call that opens it.
/// </summary>
/// <remarks>
/// <para>
/// This is a declaration, not behaviour: the listeners still live in <c>click-outside.js</c>, and
/// the portal only forwards the request. It exists because on Blazor Server each separately
/// awaited <c>InvokeAsync</c> is a network round trip, so an owner wiring its own listeners after
/// the overlay appears leaves a window — two round trips wide — in which the overlay is on screen
/// and ignores clicks outside it. On a slow link that window is long enough to notice.
/// </para>
/// <para>
/// Set <see cref="BbFloatingPortal.OnDismiss"/> to act on the gesture. The portal never closes
/// anything itself; the owner decides what a dismissal means.
/// </para>
/// </remarks>
public sealed class FloatingDismissOptions
{
    /// <summary>
    /// The <c>id</c> of the element that counts as "inside". Interaction within it is not a
    /// dismissal.
    /// </summary>
    public string ContentId { get; init; } = string.Empty;

    /// <summary>
    /// The <c>id</c> of the trigger, which also counts as "inside" — otherwise the click that
    /// closes the overlay is the same click the trigger reads as "open me", and it reopens at once.
    /// </summary>
    public string? TriggerId { get; init; }

    /// <summary>
    /// Whether to report a pointer interaction outside the content and trigger.
    /// </summary>
    public bool OnOutsideInteraction { get; init; }

    /// <summary>
    /// Whether to report Escape, watched at the document.
    /// </summary>
    /// <remarks>
    /// It has to be the document rather than the content element. Opening an overlay does not move
    /// focus — it stays on the trigger, which lives in a different subtree entirely once the
    /// content renders through the portal — so the keydown never bubbles anywhere near the content.
    /// </remarks>
    public bool OnEscapeKey { get; init; }
}
