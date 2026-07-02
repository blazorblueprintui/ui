namespace BlazorBlueprint.Components;

/// <summary>
/// Context provided to the <c>DayTemplate</c> of calendar-based components
/// (<c>BbCalendar</c>, <c>BbDatePicker</c>, <c>BbDateRangePicker</c>),
/// describing the day being rendered.
/// </summary>
public class CalendarDayContext
{
    /// <summary>
    /// The date of the day being rendered.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Whether the day is currently selected. In range selection this is true
    /// for the range start and end dates.
    /// </summary>
    public bool IsSelected { get; init; }

    /// <summary>
    /// Whether the day is disabled.
    /// </summary>
    public bool IsDisabled { get; init; }

    /// <summary>
    /// Whether the day belongs to the previous or next month relative to the
    /// displayed month.
    /// </summary>
    public bool IsOutsideMonth { get; init; }

    /// <summary>
    /// Whether the day is today.
    /// </summary>
    public bool IsToday { get; init; }

    /// <summary>
    /// Whether the day falls within the selected range. Only set in range
    /// selection (e.g. <c>BbDateRangePicker</c>); always false for single-date
    /// calendars.
    /// </summary>
    public bool IsInRange { get; init; }
}
