namespace BlazorBlueprint.Primitives;

/// <summary>
/// Controls what a focus trap focuses when it is established.
/// </summary>
/// <remarks>
/// Declared in the <c>BlazorBlueprint.Primitives</c> namespace rather than a nested one so that
/// an application importing <c>BlazorBlueprint.Components</c> can name it without ambiguity.
/// An element carrying <c>data-autofocus</c>, or an explicit target passed to
/// <c>IFocusManager.TrapFocus</c>, takes precedence over this setting.
/// </remarks>
public enum FocusTrapInitialFocus
{
    /// <summary>
    /// Focus the first tabbable descendant. This is the default and the historical behaviour.
    /// </summary>
    FirstFocusable = 0,

    /// <summary>
    /// Focus the trapping container itself. Matches the Radix default, and avoids firing an
    /// on-focus side effect belonging to the first tabbable descendant.
    /// </summary>
    Container = 1,

    /// <summary>
    /// Leave focus where it is. The trap still constrains Tab, but moves nothing on open.
    /// </summary>
    None = 2,
}
