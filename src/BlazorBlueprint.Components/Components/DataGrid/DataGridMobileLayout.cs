namespace BlazorBlueprint.Components;

/// <summary>
/// Defines how a DataGrid lays out its rows below its <c>MobileBreakpoint</c>.
/// </summary>
public enum DataGridMobileLayout
{
    /// <summary>
    /// The grid stays a table at every screen width. This is the default.
    /// </summary>
    None,

    /// <summary>
    /// Below the breakpoint each row becomes a card, and each cell shows its column's label
    /// next to its value. The header row is hidden, since every cell carries its own label.
    /// </summary>
    Stacked
}
