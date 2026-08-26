using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// An event calendar with month, week, and agenda views, generic over the consumer's event type.
/// </summary>
/// <typeparam name="TEvent">The consumer's event type. Accessor delegates map it to start/end dates and a title.</typeparam>
public partial class BbEventCalendar<TEvent> : ComponentBase
{
    private CultureInfo culture = CultureInfo.CurrentCulture;
    private Dictionary<DateTime, List<TEvent>> eventsByDay = new();
    private EventCalendarRange? lastNotifiedRange;
    private string[]? cachedDayNames;
    private DayOfWeek cachedFirstDayOfWeek;

    /// <summary>
    /// The events to display.
    /// </summary>
    [Parameter]
    public IEnumerable<TEvent>? Items { get; set; }

    /// <summary>
    /// Returns the start date/time of an event.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TEvent, DateTime> EventStart { get; set; } = default!;

    /// <summary>
    /// Returns the end date/time of an event, or null for a point-in-time event.
    /// When the delegate itself is null, all events are treated as point-in-time.
    /// Events whose end date falls on a later day than their start render a chip on each spanned day.
    /// </summary>
    [Parameter]
    public Func<TEvent, DateTime?>? EventEnd { get; set; }

    /// <summary>
    /// Returns the display title of an event.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TEvent, string> EventTitle { get; set; } = default!;

    /// <summary>
    /// Returns additional CSS classes for a specific event's chip, merged with the built-in chip classes.
    /// Use this for per-event coloring (e.g. by category).
    /// </summary>
    [Parameter]
    public Func<TEvent, string?>? EventClass { get; set; }

    /// <summary>
    /// Optional template that fully replaces the built-in event chip content.
    /// When set, only minimal layout/focus classes are applied to the chip button;
    /// combine with <see cref="EventClass"/> for chip-level styling.
    /// </summary>
    [Parameter]
    public RenderFragment<TEvent>? EventTemplate { get; set; }

    /// <summary>
    /// The active view. Use @bind-View for two-way binding.
    /// </summary>
    [Parameter]
    public EventCalendarView View { get; set; } = EventCalendarView.Month;

    /// <summary>
    /// Callback when the active view changes.
    /// </summary>
    [Parameter]
    public EventCallback<EventCalendarView> ViewChanged { get; set; }

    /// <summary>
    /// The date that controls the visible period (the month or week containing it).
    /// Use @bind-CurrentDate for two-way binding. Defaults to today.
    /// </summary>
    [Parameter]
    public DateTime CurrentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Callback when the current date changes through navigation.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime> CurrentDateChanged { get; set; }

    /// <summary>
    /// The first day of the week. Defaults to the current culture's first day of the week.
    /// </summary>
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The maximum number of event chips shown per day in the month view before
    /// the overflow "+x more" button appears. Defaults to 3.
    /// </summary>
    [Parameter]
    public int MaxEventsPerDay { get; set; } = 3;

    /// <summary>
    /// Callback when an event chip is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<TEvent> OnEventClick { get; set; }

    /// <summary>
    /// Callback when a day number (month view) or day header (week view) is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime> OnDateClick { get; set; }

    /// <summary>
    /// Callback when the visible date range changes (including the initial range).
    /// Useful for loading events on demand.
    /// </summary>
    [Parameter]
    public EventCallback<EventCalendarRange> OnViewRangeChanged { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private DayOfWeek EffectiveFirstDayOfWeek => FirstDayOfWeek ?? culture.DateTimeFormat.FirstDayOfWeek;

    private string CssClass => ClassNames.cn("w-full", Class);

    protected override async Task OnParametersSetAsync()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        if (!ReferenceEquals(culture, currentCulture) && culture.Name != currentCulture.Name)
        {
            culture = currentCulture;
            cachedDayNames = null;
        }

        if (cachedFirstDayOfWeek != EffectiveFirstDayOfWeek)
        {
            cachedFirstDayOfWeek = EffectiveFirstDayOfWeek;
            cachedDayNames = null;
        }

        await RefreshAsync();
    }

    #region Range & lookup

    private EventCalendarRange GetVisibleRange()
    {
        switch (View)
        {
            case EventCalendarView.Week:
                var weekStart = StartOfWeek(CurrentDate);
                return new EventCalendarRange(weekStart, weekStart.AddDays(6));
            case EventCalendarView.Agenda:
                var agendaStart = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                return new EventCalendarRange(agendaStart, agendaStart.AddMonths(1).AddDays(-1));
            default:
                var firstOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                var gridStart = StartOfWeek(firstOfMonth);
                return new EventCalendarRange(gridStart, gridStart.AddDays(41));
        }
    }

    private DateTime StartOfWeek(DateTime date)
    {
        var diff = ((int)date.DayOfWeek - (int)EffectiveFirstDayOfWeek + 7) % 7;
        return date.Date.AddDays(-diff);
    }

    /// <summary>
    /// Rebuilds the per-day event lookup for the visible range and raises
    /// <see cref="OnViewRangeChanged"/> when the range has changed.
    /// </summary>
    private async Task RefreshAsync()
    {
        RebuildEventLookup();

        var range = GetVisibleRange();
        if (lastNotifiedRange == range)
        {
            return;
        }

        lastNotifiedRange = range;
        if (OnViewRangeChanged.HasDelegate)
        {
            await OnViewRangeChanged.InvokeAsync(range);
        }
    }

    private void RebuildEventLookup()
    {
        eventsByDay = new Dictionary<DateTime, List<TEvent>>();
        if (Items is null || EventStart is null)
        {
            return;
        }

        var range = GetVisibleRange();
        foreach (var item in Items)
        {
            var startDay = EventStart(item).Date;
            var endDay = (EventEnd?.Invoke(item) ?? EventStart(item)).Date;
            if (endDay < startDay)
            {
                endDay = startDay;
            }

            var from = startDay < range.Start ? range.Start : startDay;
            var to = endDay > range.End ? range.End : endDay;
            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (!eventsByDay.TryGetValue(day, out var list))
                {
                    list = new List<TEvent>();
                    eventsByDay[day] = list;
                }
                list.Add(item);
            }
        }

        foreach (var list in eventsByDay.Values)
        {
            list.Sort(CompareEvents);
        }
    }

    private int CompareEvents(TEvent a, TEvent b)
    {
        // All-day events first, then by start, then by title for a stable order.
        var allDayCompare = IsAllDay(b).CompareTo(IsAllDay(a));
        if (allDayCompare != 0)
        {
            return allDayCompare;
        }

        var startCompare = EventStart(a).CompareTo(EventStart(b));
        if (startCompare != 0)
        {
            return startCompare;
        }

        return string.Compare(EventTitle(a), EventTitle(b), StringComparison.Ordinal);
    }

    private IReadOnlyList<TEvent> GetEventsForDay(DateTime day) =>
        eventsByDay.TryGetValue(day.Date, out var list) ? list : Array.Empty<TEvent>();

    #endregion

    #region View data

    private string[] DayNames => cachedDayNames ??= BuildDayNames();

    private string[] BuildDayNames()
    {
        var cultureNames = culture.DateTimeFormat.AbbreviatedDayNames;
        var start = (int)EffectiveFirstDayOfWeek;
        var names = new string[7];
        for (var i = 0; i < 7; i++)
        {
            names[i] = cultureNames[(start + i) % 7];
        }
        return names;
    }

    private IEnumerable<DateTime[]> MonthWeeks
    {
        get
        {
            var start = GetVisibleRange().Start;
            for (var week = 0; week < 6; week++)
            {
                var days = new DateTime[7];
                for (var i = 0; i < 7; i++)
                {
                    days[i] = start.AddDays((week * 7) + i);
                }
                yield return days;
            }
        }
    }

    private DateTime[] WeekDays
    {
        get
        {
            var start = StartOfWeek(CurrentDate);
            var days = new DateTime[7];
            for (var i = 0; i < 7; i++)
            {
                days[i] = start.AddDays(i);
            }
            return days;
        }
    }

    private IEnumerable<DateTime> AgendaDays => eventsByDay.Keys.OrderBy(d => d);

    private string HeaderTitle
    {
        get
        {
            if (View == EventCalendarView.Week)
            {
                var range = GetVisibleRange();
                return string.Format(
                    culture,
                    "{0} – {1}, {2}",
                    range.Start.ToString("M", culture),
                    range.End.ToString("M", culture),
                    range.End.Year);
            }

            return CurrentDate.ToString("Y", culture);
        }
    }

    private bool IsAllDay(TEvent item)
    {
        var start = EventStart(item);
        if (start.TimeOfDay != TimeSpan.Zero)
        {
            return false;
        }

        var end = EventEnd?.Invoke(item);
        if (end is null)
        {
            return true;
        }

        return end.Value.TimeOfDay == TimeSpan.Zero;
    }

    private string GetEventTimeLabel(TEvent item)
    {
        if (IsAllDay(item))
        {
            return Localizer["EventCalendar.AllDay"];
        }

        var start = EventStart(item);
        var end = EventEnd?.Invoke(item);
        if (end is null || end.Value == start)
        {
            return start.ToString("t", culture);
        }

        return string.Format(
            culture,
            "{0} – {1}",
            start.ToString("t", culture),
            end.Value.ToString("t", culture));
    }

    #endregion

    #region Navigation & interaction

    private async Task SetViewAsync(EventCalendarView view)
    {
        if (View == view)
        {
            return;
        }

        View = view;
        await ViewChanged.InvokeAsync(view);
        await RefreshAsync();
    }

    private async Task SetCurrentDateAsync(DateTime date)
    {
        CurrentDate = date.Date;
        await CurrentDateChanged.InvokeAsync(CurrentDate);
        await RefreshAsync();
    }

    private Task NavigatePreviousAsync() =>
        SetCurrentDateAsync(View == EventCalendarView.Week ? CurrentDate.AddDays(-7) : CurrentDate.AddMonths(-1));

    private Task NavigateNextAsync() =>
        SetCurrentDateAsync(View == EventCalendarView.Week ? CurrentDate.AddDays(7) : CurrentDate.AddMonths(1));

    private Task NavigateTodayAsync() => SetCurrentDateAsync(DateTime.Today);

    private Task HandleEventClick(TEvent item) => OnEventClick.InvokeAsync(item);

    private Task HandleDateClick(DateTime day) => OnDateClick.InvokeAsync(day.Date);

    /// <summary>
    /// The "+x more" overflow: navigates to the clicked day in the agenda view.
    /// </summary>
    private async Task ShowMoreAsync(DateTime day)
    {
        await SetViewAsync(EventCalendarView.Agenda);
        await SetCurrentDateAsync(day);
    }

    #endregion

    #region Styling

    private const string ChipBaseClasses = "block w-full rounded px-1.5 py-0.5 text-left text-xs font-medium bg-primary/10 text-primary hover:bg-primary/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";
    private const string ChipTemplateClasses = "block w-full rounded text-left text-xs focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";
    private const string DayNumberBaseClasses = "flex h-6 w-6 items-center justify-center rounded-full text-xs font-medium hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";
    private const string DayNumberTodayClasses = "flex h-6 w-6 items-center justify-center rounded-full text-xs font-medium bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

    private string GetEventChipClasses(TEvent item)
    {
        var baseClasses = EventTemplate is not null ? ChipTemplateClasses : ChipBaseClasses;
        var customClass = EventClass?.Invoke(item);
        return string.IsNullOrEmpty(customClass) ? baseClasses : ClassNames.cn(baseClasses, customClass);
    }

    private string GetMonthCellClasses(DateTime day)
    {
        var isOutside = day.Month != CurrentDate.Month || day.Year != CurrentDate.Year;
        return isOutside
            ? "min-h-28 bg-card p-1.5 text-muted-foreground"
            : "min-h-28 bg-card p-1.5";
    }

    private static string GetDayNumberClasses(DateTime day) =>
        day.Date == DateTime.Today ? DayNumberTodayClasses : DayNumberBaseClasses;

    private string GetViewSwitchClasses(EventCalendarView view) =>
        View == view
            ? "rounded-sm px-3 py-1 text-sm font-medium bg-primary text-primary-foreground"
            : "rounded-sm px-3 py-1 text-sm font-medium text-muted-foreground hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

    #endregion
}
