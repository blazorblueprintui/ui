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
    private readonly ExitAnimationGate exit = new();

    /// <summary>
    /// Initializes a new instance of the SheetContext.
    /// </summary>
    public SheetContext() : base(new SheetState(), "sheet")
    {
    }

    /// <summary>
    /// The exit animation registered by the content, or null when the sheet leaves the page as
    /// soon as it closes. Its argument says whether this exit is still the current one: a reopen,
    /// or a newer close, supersedes it.
    /// </summary>
    internal Func<Func<bool>, Task>? ExitAnimation
    {
        get => exit.ExitAnimation;
        set => exit.ExitAnimation = value;
    }

    /// <summary>
    /// Gets whether the sheet has closed but is still on the page, playing its exit animation.
    /// </summary>
    internal bool IsExiting => exit.IsAnimatingOut;

    /// <summary>
    /// Gets whether the sheet's portal, overlay and content should be rendered: while it is open,
    /// and while its exit animation plays.
    /// </summary>
    internal bool IsPresent => exit.IsPresent(State.IsOpen);

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
        exit.Cancel();
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the sheet, keeping its overlay and content mounted until the exit animation ends.
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
    /// Keeps the sheet on the page while its exit animation plays, then lets it go. Call this
    /// just before an open sheet's state turns closed, while its content is still rendered.
    /// </summary>
    internal void BeginExit() =>
        exit.Begin(NotifyStateChanged);

    /// <summary>
    /// Drops a running exit, so a sheet reopened mid-exit stays on the page.
    /// </summary>
    internal void CancelExit() =>
        exit.Cancel();

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
