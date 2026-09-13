namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Selects how portal-based overlay components (Dialog, and in future AlertDialog/Sheet,
/// Popover, Tooltip, etc.) render and are positioned.
/// </summary>
public enum OverlayRenderingStrategy
{
    /// <summary>
    /// Render through the Blazor portal system (scoped <see cref="PortalService"/> +
    /// Floating UI positioning over JS interop). This is the original behaviour and the
    /// default. It requires <c>&lt;BbPortalHost /&gt;</c> to live in the same render-mode
    /// scope as the interactive content that opens overlays.
    /// </summary>
    JavaScript = 0,

    /// <summary>
    /// Use the browser's native primitives (e.g. <c>&lt;dialog&gt;</c> / <c>popover</c>)
    /// which render in the top layer without a portal handshake or a shared scoped service.
    /// This makes overlays work across Blazor render-mode boundaries (e.g.
    /// <c>InteractiveWebAssembly</c>) and removes the JS focus-trap / scroll-lock /
    /// escape-key machinery for the components that opt in.
    /// </summary>
    Native = 1
}
