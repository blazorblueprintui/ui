using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components.Calendar;

/// <summary>
/// Defines the selection mode for the Calendar component.
/// </summary>
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Single is a domain term for selection mode")]
public enum CalendarMode
{
    /// <summary>
    /// Single date selection.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple individual date selection.
    /// </summary>
    Multiple,

    /// <summary>
    /// Date range selection (start and end date).
    /// </summary>
    Range
}
