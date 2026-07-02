namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Context passed to child components when AsChild=true on trigger components.
/// Allows child components to apply trigger behavior (click handlers, aria attributes).
/// </summary>
/// <remarks>
/// This enables the AsChild pattern where a trigger component doesn't render its own element,
/// but instead passes its behavior to a child component via CascadingValue.
///
/// Child components that want to act as triggers should:
/// 1. Accept [CascadingParameter(Name = "TriggerContext")] TriggerContext? TriggerContext
/// 2. Call TriggerContext?.Toggle() on click
/// 3. Apply aria attributes from TriggerContext to their rendered element
/// </remarks>
public class TriggerContext
{
    /// <summary>
    /// The unique ID for the trigger element.
    /// Should be applied as the 'id' attribute on the child element.
    /// </summary>
    public string? TriggerId { get; init; }

    /// <summary>
    /// Whether the associated overlay (dialog, dropdown, etc.) is currently open.
    /// Should be used to set aria-expanded attribute.
    /// </summary>
    public bool IsOpen { get; init; }

    /// <summary>
    /// Action to toggle the associated overlay open/closed.
    /// Should be invoked on click.
    /// </summary>
    public Action? Toggle { get; init; }

    /// <summary>
    /// Action to open the associated overlay.
    /// Used for hover-triggered components like HoverCard.
    /// </summary>
    public Action? Open { get; init; }

    /// <summary>
    /// Action to close the associated overlay.
    /// Used for hover-triggered components and explicit close.
    /// </summary>
    public Action? Close { get; init; }

    /// <summary>
    /// The value for aria-haspopup attribute.
    /// Common values: "dialog", "menu", "listbox", "true".
    /// </summary>
    public string? AriaHasPopup { get; init; }

    /// <summary>
    /// The ID of the content element that this trigger controls.
    /// Should be applied as aria-controls attribute.
    /// </summary>
    public string? AriaControls { get; init; }

    /// <summary>
    /// Keyboard event handler for triggers that need keyboard support.
    /// Used by DropdownMenuTrigger for arrow key navigation.
    /// </summary>
    public Func<Microsoft.AspNetCore.Components.Web.KeyboardEventArgs, Task>? OnKeyDown { get; init; }

    /// <summary>
    /// Mouse enter handler for hover-triggered components.
    /// </summary>
    public Action? OnMouseEnter { get; init; }

    /// <summary>
    /// Mouse leave handler for hover-triggered components.
    /// </summary>
    public Action? OnMouseLeave { get; init; }

    /// <summary>
    /// Focus handler for focus-triggered components.
    /// </summary>
    public Action? OnFocus { get; init; }

    /// <summary>
    /// Blur handler for focus-triggered components.
    /// </summary>
    public Action? OnBlur { get; init; }

    /// <summary>
    /// Action to register the trigger element reference for positioning.
    /// Child components should call this with their ElementReference after rendering.
    /// Used by components that need to position content relative to the trigger (DropdownMenu, Popover, etc.).
    /// </summary>
    public Action<Microsoft.AspNetCore.Components.ElementReference>? SetTriggerElement { get; init; }

    /// <summary>
    /// When true, the child trigger element should apply <c>pointer-events: none</c>
    /// while <see cref="IsOpen"/> is true. This mirrors the guard the non-AsChild
    /// trigger button applies, so a single gesture can't both close the overlay via
    /// click-outside detection and immediately re-open it via the trigger's click
    /// handler (Blazor Server can deliver both for one click).
    /// </summary>
    public bool SuppressPointerEventsWhenOpen { get; init; }
}
