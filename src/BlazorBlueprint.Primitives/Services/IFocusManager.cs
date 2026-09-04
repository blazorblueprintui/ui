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
    /// Traps focus within the specified container element, choosing what to focus once the trap
    /// is established.
    /// </summary>
    /// <param name="container">The container element to trap focus within.</param>
    /// <param name="initialFocus">
    /// What to focus once the trap is established. Defaults to the first tabbable descendant,
    /// which is the historical behaviour.
    /// </param>
    /// <param name="initialFocusElement">
    /// An explicit element to focus, which beats both <paramref name="initialFocus"/> and any
    /// <c>data-autofocus</c> element. Ignored if it is not inside <paramref name="container"/>.
    /// </param>
    /// <returns>A task that completes when the focus trap is established.</returns>
    /// <remarks>
    /// Resolution order is: <paramref name="initialFocusElement"/>, then an element carrying
    /// <c>data-autofocus</c> within the container, then <paramref name="initialFocus"/>.
    ///
    /// This is a default interface implementation so that adding it did not break existing
    /// implementers of <see cref="IFocusManager"/>. The default ignores both new arguments and
    /// defers to the single-argument overload, so a custom implementation keeps working exactly
    /// as before — but does not gain the initial-focus behaviour until it overrides this.
    /// </remarks>
    public Task<IAsyncDisposable> TrapFocus(
        ElementReference container,
        FocusTrapInitialFocus initialFocus,
        ElementReference? initialFocusElement = null)
        => TrapFocus(container);

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
