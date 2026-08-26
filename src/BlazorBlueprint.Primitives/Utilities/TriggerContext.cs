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
///
/// A trigger rendered with AsChild=true renders no element of its own, so a child that ignores
/// this context leaves the overlay with nothing to open it. Reading any member of this context
/// records that a child consumed it (see <see cref="NotifyConsumed"/>), which lets triggers
/// surface that mistake as a development-time warning instead of failing silently.
/// </remarks>
public class TriggerContext
{
    private readonly string? triggerId;
    private readonly bool isOpen;
    private readonly Action? toggle;
    private readonly Action? open;
    private readonly Action? close;
    private readonly string? ariaHasPopup;
    private readonly string? ariaControls;
    private readonly Func<Microsoft.AspNetCore.Components.Web.KeyboardEventArgs, Task>? onKeyDown;
    private readonly Action? onMouseEnter;
    private readonly Action? onMouseLeave;
    private readonly Action? onFocus;
    private readonly Action? onBlur;
    private readonly Action<Microsoft.AspNetCore.Components.ElementReference>? setTriggerElement;
    private readonly bool suppressPointerEventsWhenOpen;

    private bool consumed;

    /// <summary>
    /// The unique ID for the trigger element.
    /// Should be applied as the 'id' attribute on the child element.
    /// </summary>
    public string? TriggerId { get => Consume(triggerId); init => triggerId = value; }

    /// <summary>
    /// Whether the associated overlay (dialog, dropdown, etc.) is currently open.
    /// Should be used to set aria-expanded attribute.
    /// </summary>
    public bool IsOpen { get => Consume(isOpen); init => isOpen = value; }

    /// <summary>
    /// Action to toggle the associated overlay open/closed.
    /// Should be invoked on click.
    /// </summary>
    public Action? Toggle { get => Consume(toggle); init => toggle = value; }

    /// <summary>
    /// Action to open the associated overlay.
    /// Used for hover-triggered components like HoverCard.
    /// </summary>
    public Action? Open { get => Consume(open); init => open = value; }

    /// <summary>
    /// Action to close the associated overlay.
    /// Used for hover-triggered components and explicit close.
    /// </summary>
    public Action? Close { get => Consume(close); init => close = value; }

    /// <summary>
    /// The value for aria-haspopup attribute.
    /// Common values: "dialog", "menu", "listbox", "true".
    /// </summary>
    public string? AriaHasPopup { get => Consume(ariaHasPopup); init => ariaHasPopup = value; }

    /// <summary>
    /// The ID of the content element that this trigger controls.
    /// Should be applied as aria-controls attribute.
    /// </summary>
    public string? AriaControls { get => Consume(ariaControls); init => ariaControls = value; }

    /// <summary>
    /// Keyboard event handler for triggers that need keyboard support.
    /// Used by DropdownMenuTrigger for arrow key navigation.
    /// </summary>
    public Func<Microsoft.AspNetCore.Components.Web.KeyboardEventArgs, Task>? OnKeyDown { get => Consume(onKeyDown); init => onKeyDown = value; }

    /// <summary>
    /// Mouse enter handler for hover-triggered components.
    /// </summary>
    public Action? OnMouseEnter { get => Consume(onMouseEnter); init => onMouseEnter = value; }

    /// <summary>
    /// Mouse leave handler for hover-triggered components.
    /// </summary>
    public Action? OnMouseLeave { get => Consume(onMouseLeave); init => onMouseLeave = value; }

    /// <summary>
    /// Focus handler for focus-triggered components.
    /// </summary>
    public Action? OnFocus { get => Consume(onFocus); init => onFocus = value; }

    /// <summary>
    /// Blur handler for focus-triggered components.
    /// </summary>
    public Action? OnBlur { get => Consume(onBlur); init => onBlur = value; }

    /// <summary>
    /// Action to register the trigger element reference for positioning.
    /// Child components should call this with their ElementReference after rendering.
    /// Used by components that need to position content relative to the trigger (DropdownMenu, Popover, etc.).
    /// </summary>
    public Action<Microsoft.AspNetCore.Components.ElementReference>? SetTriggerElement { get => Consume(setTriggerElement); init => setTriggerElement = value; }

    /// <summary>
    /// When true, the child trigger element should apply <c>pointer-events: none</c>
    /// while <see cref="IsOpen"/> is true. This mirrors the guard the non-AsChild
    /// trigger button applies, so a single gesture can't both close the overlay via
    /// click-outside detection and immediately re-open it via the trigger's click
    /// handler (Blazor Server can deliver both for one click).
    /// </summary>
    public bool SuppressPointerEventsWhenOpen { get => Consume(suppressPointerEventsWhenOpen); init => suppressPointerEventsWhenOpen = value; }

    /// <summary>
    /// Records that a child component has taken responsibility for this trigger context.
    /// </summary>
    /// <remarks>
    /// Reading any member of this context already marks it as consumed, which covers the
    /// usual case of a child that applies the id and aria attributes while it renders.
    /// Call this explicitly from a custom trigger child that only touches the context from
    /// inside event handlers, so it is not mistaken for a child that ignores the context
    /// altogether. Calling it more than once is harmless.
    /// </remarks>
    public void NotifyConsumed()
    {
        consumed = true;
    }

    /// <summary>
    /// Whether any child has read from — or explicitly acknowledged — this context.
    /// Used by triggers to warn during development about an AsChild child that never
    /// wires the trigger behavior up, which would otherwise fail silently.
    /// </summary>
    internal bool WasConsumed => consumed;

    private T Consume<T>(T value)
    {
        consumed = true;
        return value;
    }
}
