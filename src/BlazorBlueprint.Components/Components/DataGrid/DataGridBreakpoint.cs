namespace BlazorBlueprint.Components;

/// <summary>
/// The screen width below which a DataGrid's <see cref="DataGridMobileLayout"/> applies.
/// Matches the Tailwind breakpoints of the same name.
/// </summary>
public enum DataGridBreakpoint
{
    /// <summary>
    /// Below 640px. This is the default.
    /// </summary>
    Sm,

    /// <summary>
    /// Below 768px.
    /// </summary>
    Md,

    /// <summary>
    /// Below 1024px.
    /// </summary>
    Lg
}
