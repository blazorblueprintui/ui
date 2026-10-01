using BlazorBlueprint.Primitives.Contexts;
using BlazorBlueprint.Primitives.Utilities;

namespace BlazorBlueprint.Primitives.Dialog;

/// <summary>
/// State for the Dialog primitive context.
/// </summary>
public class DialogState
{
    /// <summary>
    /// Gets or sets whether the dialog is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the element that triggered the dialog opening.
    /// Used for focus restoration on close.
    /// </summary>
    public object? TriggerElement { get; set; }
}

/// <summary>
/// Context for Dialog primitive component and its children.
/// Manages dialog state and provides IDs for ARIA attributes.
/// </summary>
public class DialogContext : PrimitiveContextWithEvents<DialogState>
{
    internal bool AllowDismiss { get; set; } = true;

    private readonly ExitAnimationGate exit = new();

    /// <summary>
    /// Initializes a new instance of the DialogContext.
    /// </summary>
    public DialogContext() : base(new DialogState(), "dialog")
    {
    }

    /// <summary>
    /// Gets the ID for the dialog trigger button.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the dialog content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets the ID for the dialog title element.
    /// </summary>
    public string TitleId => GetScopedId("title");

    /// <summary>
    /// Gets the ID for the dialog description element.
    /// </summary>
    public string DescriptionId => GetScopedId("description");

    /// <summary>
    /// Gets whether the dialog is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets whether the dialog is playing its closed-state exit animation. While set, the
    /// overlay and content stay mounted with <c>data-state="closed"</c> so their animate-out
    /// classes get a window to run in, then <see cref="CompleteClose"/> clears it to unmount.
    /// </summary>
    internal bool IsAnimatingOut => exit.IsAnimatingOut;

    /// <summary>
    /// Gets whether the dialog should be present in the DOM: open, or playing its exit animation.
    /// </summary>
    internal bool IsPresent => exit.IsPresent(State.IsOpen);

    /// <summary>
    /// Opens the dialog.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the dialog.</param>
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
    /// Closes the dialog, keeping the overlay and content mounted for their exit animation.
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
    /// dialog is open, so a reopen that beats the animation to the punch keeps the dialog mounted.
    /// </summary>
    internal void CompleteClose()
    {
        if (exit.CompleteClose(State.IsOpen))
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Toggles the dialog open/closed state.
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
}
