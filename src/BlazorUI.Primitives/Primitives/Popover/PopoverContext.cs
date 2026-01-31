using Microsoft.AspNetCore.Components;
using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.Popover;

/// <summary>
/// State for the Popover primitive context.
/// </summary>
public class PopoverState
{
    /// <summary>
    /// Gets or sets whether the popover is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the element that triggered the popover opening.
    /// Used for positioning and focus management.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }
}

/// <summary>
/// Context for Popover primitive component and its children.
/// Manages popover state and provides IDs for ARIA attributes.
/// </summary>
public class PopoverContext : PrimitiveContextWithEvents<PopoverState>
{
    /// <summary>
    /// Initializes a new instance of the PopoverContext.
    /// </summary>
    public PopoverContext() : base(new PopoverState(), "popover")
    {
    }

    /// <summary>
    /// Gets the ID for the popover trigger button.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the popover content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets whether the popover is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Opens the popover.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the popover.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
        });
    }

    /// <summary>
    /// Closes the popover.
    /// </summary>
    public void Close()
    {
        UpdateState(state =>
        {
            state.IsOpen = false;
        });
    }

    /// <summary>
    /// Toggles the popover open/closed state.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the toggle.</param>
    public void Toggle(ElementReference? triggerElement = null)
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
    /// Event fired when the popover content is fully ready (positioned and visible).
    /// </summary>
    public event Action? OnContentReady;

    /// <summary>
    /// Notifies subscribers that the popover content is fully ready.
    /// Called by PopoverContent after setup completes.
    /// </summary>
    public void NotifyContentReady()
    {
        OnContentReady?.Invoke();
    }

    /// <summary>
    /// Sets the trigger element reference for positioning.
    /// This is called by PopoverTrigger to ensure the element is available
    /// even when the popover is opened programmatically.
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
