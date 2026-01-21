namespace BlazorUI.Components.Calendar;

/// <summary>
/// Defines the current view mode for the Calendar component's navigation.
/// </summary>
public enum CalendarViewMode
{
    /// <summary>
    /// Shows the day grid for selecting a specific date.
    /// </summary>
    Days,

    /// <summary>
    /// Shows the month grid for selecting a month.
    /// </summary>
    Months,

    /// <summary>
    /// Shows the year grid for selecting a year.
    /// </summary>
    Years
}
