using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Implementation of portal rendering service for Blazor.
/// Manages a registry of portals that can be rendered at document body level.
/// </summary>
public class PortalService : IPortalService
{
    private readonly ConcurrentDictionary<string, RenderFragment> _portals = new();

    /// <inheritdoc />
    public event Action? OnPortalsChanged;

    /// <inheritdoc />
    public event Action<string>? OnPortalRendered;

    /// <inheritdoc />
    public void NotifyPortalRendered(string portalId) =>
        OnPortalRendered?.Invoke(portalId);

    /// <inheritdoc />
    public void RegisterPortal(string id, RenderFragment content)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Portal ID cannot be null or whitespace.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(content);

        _portals[id] = content;
        OnPortalsChanged?.Invoke();
    }

    /// <inheritdoc />
    public void UnregisterPortal(string id)
    {
        if (_portals.TryRemove(id, out _))
        {
            OnPortalsChanged?.Invoke();
        }
    }

    /// <inheritdoc />
    public void UpdatePortalContent(string id, RenderFragment content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!_portals.TryUpdate(id, content, _portals.GetValueOrDefault(id)!))
        {
            throw new InvalidOperationException($"Portal with ID '{id}' is not registered.");
        }

        OnPortalsChanged?.Invoke();
    }

    /// <inheritdoc />
    public void RefreshPortal(string id)
    {
        if (_portals.ContainsKey(id))
        {
            // Notify PortalHost to re-render WITHOUT replacing the RenderFragment
            // This allows the existing fragment to pick up new captured values
            // without creating new DOM elements (which would break ElementReference)
            OnPortalsChanged?.Invoke();
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, RenderFragment> GetPortals() =>
        _portals;
}
