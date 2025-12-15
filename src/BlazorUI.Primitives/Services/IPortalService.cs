using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.Services;

/// <summary>
/// Service for rendering content at the document body level, outside the normal DOM hierarchy.
/// Enables proper z-index stacking for overlays like dialogs, popovers, and dropdowns.
/// </summary>
public interface IPortalService
{
    /// <summary>
    /// Event raised when the portal registry changes (portal added, updated, or removed).
    /// </summary>
    event Action? OnPortalsChanged;

    /// <summary>
    /// Event raised when a specific portal has been rendered in the DOM.
    /// Used for synchronization between content components and PortalHost.
    /// </summary>
    event Action<string>? OnPortalRendered;

    /// <summary>
    /// Notifies that a portal has been rendered in the DOM.
    /// Called by PortalHost after rendering portal content.
    /// </summary>
    /// <param name="portalId">The ID of the portal that was rendered.</param>
    void NotifyPortalRendered(string portalId);

    /// <summary>
    /// Registers a new portal with the specified ID and content.
    /// </summary>
    /// <param name="id">Unique identifier for the portal.</param>
    /// <param name="content">Content to render in the portal.</param>
    void RegisterPortal(string id, RenderFragment content);

    /// <summary>
    /// Unregisters a portal by ID, removing it from rendering.
    /// </summary>
    /// <param name="id">The portal ID to remove.</param>
    void UnregisterPortal(string id);

    /// <summary>
    /// Updates the content of an existing portal.
    /// </summary>
    /// <param name="id">The portal ID to update.</param>
    /// <param name="content">New content to render.</param>
    void UpdatePortalContent(string id, RenderFragment content);

    /// <summary>
    /// Gets all registered portals.
    /// </summary>
    /// <returns>Dictionary of portal IDs to their render fragments.</returns>
    IReadOnlyDictionary<string, RenderFragment> GetPortals();
}
