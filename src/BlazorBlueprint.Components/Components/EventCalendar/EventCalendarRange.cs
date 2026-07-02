namespace BlazorBlueprint.Components;

/// <summary>
/// The inclusive date range currently visible in a <see cref="BbEventCalendar{TEvent}"/> view.
/// </summary>
/// <param name="Start">The first visible day (date component only).</param>
/// <param name="End">The last visible day (date component only).</param>
public readonly record struct EventCalendarRange(DateTime Start, DateTime End);
