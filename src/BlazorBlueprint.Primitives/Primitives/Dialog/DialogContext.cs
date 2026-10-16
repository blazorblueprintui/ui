using BlazorBlueprint.Primitives.Contexts;

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

    /// <summary>
    /// The exit animation registered by the content, or null when the dialog leaves the page as
    /// soon as it closes. Its argument says whether this exit is still the current one: a reopen,
    /// or a newer close, supersedes it.
    /// </summary>
    internal Func<Func<bool>, Task>? ExitAnimation { get; set; }

    /// <summary>
    /// Whether the dialog has closed but is still on the page, playing its exit animation.
    /// </summary>
    internal bool IsExiting { get; private set; }

    /// <summary>
    /// Whether the dialog's portal, overlay and content should be rendered: while it is open, and
    /// while its exit animation plays.
    /// </summary>
    internal bool IsPresent => State.IsOpen || IsExiting;

    private int exitVersion;

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
    /// Opens the dialog.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the dialog.</param>
    public void Open(object? triggerElement = null)
    {
        CancelExit();
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public void Close()
    {
        if (State.IsOpen)
        {
            BeginExit();
        }

        UpdateState(state => state.IsOpen = false);
    }

    /// <summary>
    /// Keeps the dialog on the page while its exit animation plays, then lets it go. Call this
    /// just before an open dialog's state turns closed, while its content is still rendered.
    /// </summary>
    internal void BeginExit()
    {
        if (ExitAnimation is not { } exit)
        {
            return;
        }

        IsExiting = true;
        _ = FinishExitAsync(exit, ++exitVersion);
    }

    /// <summary>
    /// Drops a running exit, so a dialog reopened mid-exit stays on the page.
    /// </summary>
    internal void CancelExit()
    {
        exitVersion++;
        IsExiting = false;
    }

    private async Task FinishExitAsync(Func<Func<bool>, Task> exit, int version)
    {
        try
        {
            await exit(() => version == exitVersion);
        }
        finally
        {
            // A reopen, or a newer close, owns presence now. An exit that ends at once does so
            // before the open state turns closed, so it must not wait for that state here.
            if (version == exitVersion)
            {
                IsExiting = false;
                NotifyStateChanged();
            }
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
