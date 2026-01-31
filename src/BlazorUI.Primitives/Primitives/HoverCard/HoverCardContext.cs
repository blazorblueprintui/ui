using Microsoft.AspNetCore.Components;
using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.HoverCard;

/// <summary>
/// State for the HoverCard primitive context.
/// </summary>
public class HoverCardState
{
    /// <summary>
    /// Gets or sets whether the hover card is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the delay before opening (in milliseconds).
    /// </summary>
    public int OpenDelay { get; set; } = 700;

    /// <summary>
    /// Gets or sets the delay before closing (in milliseconds).
    /// </summary>
    public int CloseDelay { get; set; } = 300;

    /// <summary>
    /// Gets or sets the element that triggered the hover card opening.
    /// Used for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }
}

/// <summary>
/// Context for HoverCard primitive component and its children.
/// Manages hover card state and provides IDs for ARIA attributes.
/// </summary>
public class HoverCardContext : PrimitiveContextWithEvents<HoverCardState>
{
    /// <summary>
    /// Initializes a new instance of the HoverCardContext.
    /// </summary>
    public HoverCardContext() : base(new HoverCardState(), "hovercard")
    {
    }

    /// <summary>
    /// Gets the ID for the hover card trigger element.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the hover card content element.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets whether the hover card is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets the open delay in milliseconds.
    /// </summary>
    public int OpenDelay => State.OpenDelay;

    /// <summary>
    /// Gets the close delay in milliseconds.
    /// </summary>
    public int CloseDelay => State.CloseDelay;

    /// <summary>
    /// Opens the hover card.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the hover card.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the hover card.
    /// </summary>
    public void Close()
    {
        UpdateState(state =>
        {
            state.IsOpen = false;
        });
    }

    /// <summary>
    /// Sets the open state of the hover card.
    /// </summary>
    public void SetOpen(bool isOpen)
    {
        UpdateState(state =>
        {
            state.IsOpen = isOpen;
        });
    }

    /// <summary>
    /// Sets the trigger element reference for positioning.
    /// This is called by HoverCardTrigger to ensure the element is available
    /// even when the hover card is opened programmatically.
    /// </summary>
    /// <param name="triggerElement">The trigger element reference.</param>
    public void SetTriggerElement(ElementReference triggerElement)
    {
        // Only update and notify if the element reference actually changed
        // This prevents infinite re-render loops while ensuring the content
        // gets notified when the trigger element becomes available
        if (State.TriggerElement?.Id != triggerElement.Id)
        {
            UpdateState(state =>
            {
                state.TriggerElement = triggerElement;
            });
        }
    }
}
