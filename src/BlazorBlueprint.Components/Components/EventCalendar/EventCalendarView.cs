namespace BlazorBlueprint.Components;

/// <summary>
/// The available views for <see cref="BbEventCalendar{TEvent}"/>.
/// </summary>
public enum EventCalendarView
{
    /// <summary>
    /// A six-week month grid with event chips per day.
    /// </summary>
    Month,

    /// <summary>
    /// Seven day columns listing the week's events per day in start order.
    /// </summary>
    Week,

    /// <summary>
    /// A chronological list of the current month's events grouped by day.
    /// </summary>
    Agenda
}
