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
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public void Close() =>
        UpdateState(state => state.IsOpen = false);

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
