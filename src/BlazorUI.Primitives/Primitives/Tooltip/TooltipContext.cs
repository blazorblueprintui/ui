using Microsoft.AspNetCore.Components;
using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.Tooltip;

/// <summary>
/// Holds the state for a Tooltip primitive component.
/// </summary>
public class TooltipState
{
    /// <summary>
    /// Whether the tooltip is currently visible.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// The trigger element reference for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }

    /// <summary>
    /// The placement position for the tooltip.
    /// </summary>
    public string Placement { get; set; } = "top";
}

/// <summary>
/// Context for the Tooltip primitive component system.
/// Manages tooltip visibility state, timing delays, and provides scoped IDs for ARIA.
/// </summary>
public class TooltipContext : PrimitiveContextWithEvents<TooltipState>
{
    /// <summary>
    /// Initializes a new instance of the TooltipContext.
    /// </summary>
    public TooltipContext() : base(new TooltipState(), "tooltip")
    {
    }

    /// <summary>
    /// Gets a value indicating whether the tooltip is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets the placement position for the tooltip.
    /// </summary>
    public string Placement => State.Placement;

    /// <summary>
    /// Gets the scoped ID for the trigger element.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the scoped ID for the tooltip content.
    /// </summary>
    public string TooltipId => GetScopedId("content");

    /// <summary>
    /// Signals intent to open the tooltip (without immediately updating IsOpen).
    /// The Tooltip component will apply the delay before actually opening.
    /// </summary>
    /// <param name="triggerElement">Optional trigger element reference for positioning.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        UpdateState(state =>
        {
            // Store trigger element but don't change IsOpen yet
            // The Tooltip component will handle the delay
            if (triggerElement.HasValue)
            {
                state.TriggerElement = triggerElement.Value;
            }
            // Set a flag to signal open intent
            state.IsOpen = true; // This will trigger the delay logic in Tooltip component
        });
    }

    /// <summary>
    /// Signals intent to close the tooltip (without immediately updating IsOpen).
    /// The Tooltip component will apply the delay before actually closing.
    /// </summary>
    public void Close()
    {
        UpdateState(state =>
        {
            // Signal close intent
            state.IsOpen = false; // This will trigger the delay logic in Tooltip component
        });
    }

    /// <summary>
    /// Sets the placement for the tooltip.
    /// </summary>
    /// <param name="placement">The placement position ("top", "bottom", "left", "right").</param>
    public void SetPlacement(string placement)
    {
        UpdateState(state =>
        {
            state.Placement = placement;
        });
    }

    /// <summary>
    /// Sets the trigger element reference for positioning.
    /// This is called by TooltipTrigger to ensure the element is available
    /// even when the tooltip is opened programmatically.
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
