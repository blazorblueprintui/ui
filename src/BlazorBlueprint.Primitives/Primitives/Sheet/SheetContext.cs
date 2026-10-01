using BlazorBlueprint.Primitives.Contexts;
using BlazorBlueprint.Primitives.Utilities;

namespace BlazorBlueprint.Primitives.Sheet;

/// <summary>
/// State for the Sheet primitive context.
/// </summary>
public class SheetState
{
    /// <summary>
    /// Gets or sets whether the sheet is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the side from which the sheet slides in.
    /// </summary>
    public SheetSide Side { get; set; } = SheetSide.Right;

    /// <summary>
    /// Gets or sets whether the sheet can be dismissed by clicking the overlay or pressing Escape.
    /// </summary>
    public bool Modal { get; set; } = true;

    /// <summary>
    /// Gets or sets the element that triggered the sheet opening.
    /// Used for focus restoration on close.
    /// </summary>
    public object? TriggerElement { get; set; }
}

/// <summary>
/// Context for Sheet primitive component and its children.
/// Manages sheet state and provides IDs for ARIA attributes.
/// </summary>
public class SheetContext : PrimitiveContextWithEvents<SheetState>
{
    /// <summary>
    /// Initializes a new instance of the SheetContext.
    /// </summary>
    public SheetContext() : base(new SheetState(), "sheet")
    {
    }

    private readonly ExitAnimationGate exit = new();

    /// <summary>
    /// Gets the ID for the sheet trigger button.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the sheet content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets the ID for the sheet title element.
    /// </summary>
    public string TitleId => GetScopedId("title");

    /// <summary>
    /// Gets the ID for the sheet description element.
    /// </summary>
    public string DescriptionId => GetScopedId("description");

    /// <summary>
    /// Gets the ID for the sheet overlay element.
    /// </summary>
    public string OverlayId => GetScopedId("overlay");

    /// <summary>
    /// Gets whether the sheet is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets whether the sheet is playing its closed-state exit animation. While set, the
    /// overlay and content stay mounted with <c>data-state="closed"</c> so their animate-out
    /// classes get a window to run in, then <see cref="CompleteClose"/> clears it to unmount.
    /// </summary>
    internal bool IsAnimatingOut => exit.IsAnimatingOut;

    /// <summary>
    /// Gets whether the sheet should be present in the DOM: open, or playing its exit animation.
    /// </summary>
    internal bool IsPresent => exit.IsPresent(State.IsOpen);

    /// <summary>
    /// Gets the side from which the sheet slides in.
    /// </summary>
    public SheetSide Side => State.Side;

    /// <summary>
    /// Gets whether the sheet can be dismissed by clicking the overlay or pressing Escape.
    /// </summary>
    public bool Modal => State.Modal;

    /// <summary>
    /// Opens the sheet.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the sheet.</param>
    public void Open(object? triggerElement = null)
    {
        exit.Cancel();
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the sheet, keeping the overlay and content mounted for their exit animation.
    /// </summary>
    public void Close()
    {
        if (!State.IsOpen)
        {
            return;
        }

        exit.Begin();
        UpdateState(state => state.IsOpen = false);
    }

    /// <summary>
    /// Advances the state in response to a controlled <c>Open</c> parameter change, running the
    /// same open/close transition logic as the unmanaged methods.
    /// </summary>
    internal void SetIsOpen(bool isOpen)
    {
        if (State.IsOpen == isOpen)
        {
            return;
        }

        if (isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    /// <summary>
    /// Ends the exit animation, allowing the overlay and content to unmount. No-op while the
    /// sheet is open, so a reopen that beats the animation to the punch keeps the sheet mounted.
    /// </summary>
    internal void CompleteClose()
    {
        if (exit.CompleteClose(State.IsOpen))
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Toggles the sheet open/closed state.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the toggle.</param>
    public void Toggle(object? triggerElement = null)
    {
        if (State.IsOpen)
        {
            Close();
        }
        else
        {
            Open(triggerElement);
        }
    }

    /// <summary>
    /// Sets the side from which the sheet slides in.
    /// </summary>
    /// <param name="side">The side to slide in from.</param>
    public void SetSide(SheetSide side) =>
        UpdateState(state => state.Side = side);
}
