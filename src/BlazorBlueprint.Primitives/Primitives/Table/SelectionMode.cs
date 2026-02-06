using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Defines the selection behavior for table rows.
/// </summary>
public enum SelectionMode
{
    /// <summary>
    /// No row selection allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Only one row can be selected at a time.
    /// </summary>
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single is a domain term for selection mode")]
    Single = 1,

    /// <summary>
    /// Multiple rows can be selected simultaneously.
    /// </summary>
    Multiple = 2
}
