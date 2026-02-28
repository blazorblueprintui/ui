using BlazorBlueprint.Primitives.Contexts;

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
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the sheet.
    /// </summary>
    public void Close() =>
        UpdateState(state => state.IsOpen = false);

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
