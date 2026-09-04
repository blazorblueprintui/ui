namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// The modifier keys held during a row click, which decide how
/// <see cref="DataGridSelectionBehavior.Replace"/> changes the selection.
/// </summary>
/// <param name="Extend">
/// True when Shift was held: select the range from the anchor row to the clicked row.
/// </param>
/// <param name="Additive">
/// True when Ctrl (or Cmd on macOS) was held: add or remove the clicked row only.
/// </param>
public readonly record struct RowSelectionModifiers(bool Extend, bool Additive)
{
    /// <summary>
    /// Gets the modifiers for a click with no modifier key held.
    /// </summary>
    public static RowSelectionModifiers None => new(false, false);
}
