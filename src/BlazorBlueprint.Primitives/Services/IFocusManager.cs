using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Service for managing focus within components, including focus trapping for modal dialogs.
/// </summary>
public interface IFocusManager
{
    /// <summary>
    /// Traps focus within the specified container element.
    /// Tab and Shift+Tab will cycle focus within the container only.
    /// </summary>
    /// <param name="container">The container element to trap focus within.</param>
    /// <returns>A task that completes when the focus trap is established.</returns>
    public Task<IAsyncDisposable> TrapFocus(ElementReference container);

    /// <summary>
    /// Restores focus to the previously focused element.
    /// Typically used when closing a dialog to return focus to the trigger.
    /// </summary>
    /// <param name="previousElement">The element to restore focus to, or null to do nothing.</param>
    /// <returns>A task that completes when focus is restored.</returns>
    public Task RestoreFocus(ElementReference? previousElement);

    /// <summary>
    /// Focuses the first focusable element within the container.
    /// </summary>
    /// <param name="container">The container to search for focusable elements.</param>
    /// <returns>A task that completes when focus is set.</returns>
    public Task FocusFirst(ElementReference container);

    /// <summary>
    /// Focuses the last focusable element within the container.
    /// </summary>
    /// <param name="container">The container to search for focusable elements.</param>
    /// <returns>A task that completes when focus is set.</returns>
    public Task FocusLast(ElementReference container);
}
