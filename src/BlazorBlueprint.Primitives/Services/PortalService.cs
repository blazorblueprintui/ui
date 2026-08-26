using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Implementation of portal rendering service for Blazor.
/// Manages a categorized registry of portals that can be rendered at document body level.
/// Portals are split into Container (Dialog, Sheet) and Overlay (Popover, Select, etc.)
/// categories, each with insertion-order tracking for predictable rendering.
/// </summary>
public class PortalService(ILogger<PortalService> logger) : IPortalService
{
    private static readonly Action<ILogger, Exception?> logMissingPortalHost =
        LoggerMessage.Define(LogLevel.Warning, new EventId(1, "MissingPortalHost"),
            "BlazorBlueprint: no <BbPortalHost /> is registered in this render context, so Dialog, Select, " +
            "Popover and other portal-based components cannot render. " +
            "If your layout has no <BbPortalHost />, add one to MainLayout.razor. " +
            "If it already has one, then it is rendering somewhere this code cannot reach it. In a Blazor Web " +
            "App that is usually because the layout is static while the page is interactive: a routed page's " +
            "layout inherits the page's render mode only when interactivity is applied globally. Either set " +
            "@rendermode on <Routes /> in App.razor, or render <BbPortalHost />, <BbToastProvider /> and " +
            "<BbDialogProvider /> as interactive islands with their own @rendermode. Under " +
            "InteractiveWebAssembly the host must additionally live in an assembly the client project loads — " +
            "a layout in the server project cannot run in the browser, however it is marked.");

    private readonly ConcurrentDictionary<string, PortalEntry> portals = new();
    private long nextOrder;
    private bool hasWarnedMissingHost;

    /// <inheritdoc />
    public bool HasHost { get; private set; }

    /// <inheritdoc />
    public event Action<PortalCategory>? OnPortalsCategoryChanged;

    /// <inheritdoc />
    public event Action<string>? OnPortalRendered;

    /// <inheritdoc />
    public void RegisterHost() => HasHost = true;

    /// <inheritdoc />
    public void UnregisterHost() => HasHost = false;

    /// <inheritdoc />
    public void NotifyPortalRendered(string portalId) =>
        OnPortalRendered?.Invoke(portalId);

    /// <inheritdoc />
    public void RegisterPortal(string id, RenderFragment content, PortalCategory category)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Portal ID cannot be null or whitespace.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(content);

        if (!HasHost && !hasWarnedMissingHost)
        {
            hasWarnedMissingHost = true;
            logMissingPortalHost(logger, null);
        }

        var entry = new PortalEntry
        {
            Content = content,
            Category = category,
            Order = Interlocked.Increment(ref nextOrder)
        };

        portals[id] = entry;
        OnPortalsCategoryChanged?.Invoke(category);
    }

    /// <inheritdoc />
    public void UnregisterPortal(string id)
    {
        if (portals.TryRemove(id, out var entry))
        {
            OnPortalsCategoryChanged?.Invoke(entry.Category);
        }
    }

    /// <inheritdoc />
    public void UpdatePortalContent(string id, RenderFragment content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!portals.TryGetValue(id, out var entry))
        {
            throw new InvalidOperationException($"Portal with ID '{id}' is not registered.");
        }

        // Update content while preserving category and insertion order
        entry.Content = content;
        OnPortalsCategoryChanged?.Invoke(entry.Category);
    }

    /// <inheritdoc />
    public void RefreshPortal(string id)
    {
        if (portals.TryGetValue(id, out var entry))
        {
            // Notify the category host to re-render WITHOUT replacing the RenderFragment.
            // This allows the existing fragment to pick up new captured values
            // without creating new DOM elements (which would break ElementReference).
            OnPortalsCategoryChanged?.Invoke(entry.Category);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<KeyValuePair<string, RenderFragment>> GetPortals(PortalCategory category)
    {
        return portals
            .Where(kvp => kvp.Value.Category == category)
            .OrderBy(kvp => kvp.Value.Order)
            .Select(kvp => new KeyValuePair<string, RenderFragment>(kvp.Key, kvp.Value.Content))
            .ToList();
    }
}
