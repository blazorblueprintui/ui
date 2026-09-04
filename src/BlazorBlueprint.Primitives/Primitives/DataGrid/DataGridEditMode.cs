// In the BlazorBlueprint.Primitives namespace rather than BlazorBlueprint.Primitives.DataGrid,
// alongside SortDirection and DataGridSelectionBehavior: an app that imports
// BlazorBlueprint.Components cannot also import BlazorBlueprint.Primitives.DataGrid without making
// the name BbDataGrid ambiguous, so an enum a consumer has to name in markup goes here.
namespace BlazorBlueprint.Primitives;

/// <summary>
/// Defines how a DataGrid lets a user edit rows in place.
/// </summary>
public enum DataGridEditMode
{
    /// <summary>
    /// Rows cannot be edited. This is the default.
    /// </summary>
    None,

    /// <summary>
    /// A whole row goes into edit at once: every editable cell becomes an input, and the changes
    /// are committed or discarded together.
    /// </summary>
    Row
}
