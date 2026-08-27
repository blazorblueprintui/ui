// Deliberately in the BlazorBlueprint.Primitives namespace rather than
// BlazorBlueprint.Primitives.DataGrid, alongside SortDirection. An app that imports
// BlazorBlueprint.Components cannot also import BlazorBlueprint.Primitives.DataGrid without
// making the name BbDataGrid ambiguous, so an enum a consumer has to name in markup goes here.
namespace BlazorBlueprint.Primitives;

using BlazorBlueprint.Primitives.Table;


/// <summary>
/// Defines how clicking a row changes the selection.
/// </summary>
public enum DataGridSelectionBehavior
{
    /// <summary>
    /// Each click flips one row on or off and leaves every other row alone.
    /// Modifier keys are ignored. This is the default.
    /// </summary>
    Toggle,

    /// <summary>
    /// Clicking replaces the selection, the way a file explorer or a spreadsheet does.
    /// A plain click selects only the clicked row, Shift+Click selects the range from the
    /// anchor row, and Ctrl+Click (Cmd+Click on macOS) adds or removes one row without
    /// disturbing the rest.
    /// </summary>
    /// <remarks>
    /// Range and additive selection need <see cref="SelectionMode.Multiple"/>. Under
    /// <see cref="SelectionMode.Single"/> every click behaves as a plain click.
    /// Checkbox clicks in a select column keep <see cref="Toggle"/> semantics under this
    /// behaviour too, because a checkbox that cleared the rest of the selection would be
    /// unusable.
    /// </remarks>
    Replace
}
