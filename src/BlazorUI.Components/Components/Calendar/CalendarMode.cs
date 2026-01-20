namespace BlazorUI.Components.Calendar;

/// <summary>
/// Defines the selection mode for the Calendar component.
/// </summary>
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
