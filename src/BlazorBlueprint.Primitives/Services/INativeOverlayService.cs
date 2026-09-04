using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Resolves the effective <see cref="OverlayRenderingStrategy"/> for a component and drives
/// the browser's native overlay primitives (currently the <c>&lt;dialog&gt;</c> element).
/// </summary>
public interface INativeOverlayService
{
    /// <summary>
    /// Gets whether the browser supports the native <c>&lt;dialog&gt;</c> element's
    /// <c>showModal()</c>. Resolved once per scope and cached. Returns false when JS interop
    /// is unavailable (e.g. during prerendering). Used for diagnostics when native is requested.
    /// </summary>
    Task<bool> IsDialogSupportedAsync();

    /// <summary>
    /// Resolves the strategy a component should render with, synchronously (safe to call during
    /// render). A non-null <paramref name="requested"/> (the component's own parameter) wins;
    /// otherwise the global default applies.
    /// </summary>
    OverlayRenderingStrategy ResolveStrategy(OverlayRenderingStrategy? requested);

    /// <summary>
    /// Opens a <c>&lt;dialog&gt;</c> element as a modal (top layer).
    /// </summary>
    Task ShowDialogAsync(ElementReference element);

    /// <summary>
    /// Closes a <c>&lt;dialog&gt;</c> element.
    /// </summary>
    Task CloseDialogAsync(ElementReference element, string? returnValue = null);

    /// <summary>
    /// Focuses a dialog's content after opening (falls back to first focusable element).
    /// </summary>
    Task FocusDialogAsync(ElementReference element);

    /// <summary>
    /// Restores focus to the element that opened the dialog.
    /// </summary>
    Task FocusTriggerAsync(ElementReference element);

    /// <summary>
    /// Wires native <c>&lt;dialog&gt;</c> lifecycle events (Escape/cancel, close, backdrop click)
    /// to a .NET instance. The returned handle must be disposed to remove the listeners.
    /// </summary>
    Task<IAsyncDisposable> SetupDialogAsync(ElementReference element, object dotNetRef);
}
