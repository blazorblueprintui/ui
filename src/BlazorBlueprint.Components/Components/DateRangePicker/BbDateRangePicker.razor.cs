using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A date range picker component with two-month calendar view.
/// </summary>
public partial class BbDateRangePicker : ComponentBase
{
    private bool _isOpen;
    private DateTime _displayMonth1;
    private DateTime _displayMonth2;
    private DateTime? _selectionStart;
    private DateTime? _selectionEnd;

    // OnParametersSet guard
    private DateRange? _previousValue;

    // Caching fields
    private DayOfWeek _cachedFirstDayOfWeek;
    private string[]? _cachedDayNames;
    private List<List<DateTime?>>? _cachedWeeksMonth1;
    private List<List<DateTime?>>? _cachedWeeksMonth2;

    // ShouldRender tracking fields
    private bool _lastIsOpen;
    private DateTime _lastDisplayMonth1;
    private DateTime _lastDisplayMonth2;
    private DateTime? _lastSelectionStart;
    private DateTime? _lastSelectionEnd;
    private DateRange? _lastValue;
    private DateTime? _lastMinDate;
    private DateTime? _lastMaxDate;
    private bool _lastShowTwoMonths;
    private bool _lastShowPresets;
    private bool _lastDisabled;

    /// <summary>
    /// The selected date range.
    /// </summary>
    [Parameter]
    public DateRange? Value { get; set; }

    /// <summary>
    /// Callback when the date range changes.
    /// </summary>
    [Parameter]
    public EventCallback<DateRange?> ValueChanged { get; set; }

    /// <summary>
    /// The minimum selectable date.
    /// </summary>
    [Parameter]
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// The maximum selectable date.
    /// </summary>
    [Parameter]
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Function to determine if a date is disabled.
    /// </summary>
    [Parameter]
    public Func<DateTime, bool>? DisabledDates { get; set; }

    /// <summary>
    /// The minimum number of days that must be selected.
    /// </summary>
    [Parameter]
    public int? MinDays { get; set; }

    /// <summary>
    /// The maximum number of days that can be selected.
    /// </summary>
    [Parameter]
    public int? MaxDays { get; set; }

    /// <summary>
    /// Whether to show two months side by side.
    /// </summary>
    [Parameter]
    public bool ShowTwoMonths { get; set; } = true;

    /// <summary>
    /// Whether to show preset date ranges.
    /// </summary>
    [Parameter]
    public bool ShowPresets { get; set; } = true;

    /// <summary>
    /// The first day of the week.
    /// </summary>
    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

    /// <summary>
    /// The date format for display.
    /// </summary>
    [Parameter]
    public string DateFormat { get; set; } = "MMM d, yyyy";

    /// <summary>
    /// Placeholder text when no range is selected.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select date range";

    /// <summary>
    /// Whether the picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Additional CSS classes for the trigger button.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    private static readonly string[] BaseDayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

    private string[] DayNames => _cachedDayNames ??= BuildDayNames();

    private string[] BuildDayNames()
    {
        var start = (int)FirstDayOfWeek;
        var result = new string[7];
        for (var i = 0; i < 7; i++)
        {
            result[i] = BaseDayNames[(start + i) % 7];
        }
        return result;
    }

    private static readonly string[] MonthNames = { "January", "February", "March", "April", "May", "June",
                                                     "July", "August", "September", "October", "November", "December" };

    // Pre-computed CSS class constants — eliminates ~168 ClassNames.cn()/TailwindMerge calls per render
    private const string CellEmpty = "h-9 w-9 flex-1 text-center text-sm p-0";
    private const string CellDefault = "h-9 w-9 flex-1 text-center text-sm p-0 relative";
    private const string CellInRange = "h-9 w-9 flex-1 text-center text-sm p-0 relative bg-accent";
    private const string CellRangeStart = "h-9 w-9 flex-1 text-center text-sm p-0 relative rounded-l-md bg-accent";
    private const string CellRangeEnd = "h-9 w-9 flex-1 text-center text-sm p-0 relative rounded-r-md bg-accent";

    private const string DayBase = "inline-flex h-9 w-full items-center justify-center rounded-md text-sm font-normal ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50";
    private const string DayDefault = DayBase + " hover:bg-accent hover:text-accent-foreground";
    private const string DayDisabled = DayBase + " text-muted-foreground opacity-50";
    private const string DayRangeEndpoint = DayBase + " bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground";
    private const string DayInRange = DayBase + " bg-accent text-accent-foreground";
    private const string DayToday = DayBase + " bg-accent text-accent-foreground";

    private bool CanApply => _selectionStart.HasValue && _selectionEnd.HasValue;

    protected override void OnInitialized()
    {
        InitializeDisplayMonths();
        if (Value != null)
        {
            _selectionStart = Value.Start;
            _selectionEnd = Value.End;
        }
        _previousValue = Value;
    }

    protected override void OnParametersSet()
    {
        // Only sync when Value actually changes from external source
        if (Value != _previousValue)
        {
            _previousValue = Value;
            if (Value != null)
            {
                _selectionStart = Value.Start;
                _selectionEnd = Value.End;
            }
        }

        // Invalidate caches when FirstDayOfWeek changes
        if (_cachedFirstDayOfWeek != FirstDayOfWeek)
        {
            _cachedFirstDayOfWeek = FirstDayOfWeek;
            _cachedDayNames = null;
            _cachedWeeksMonth1 = null;
            _cachedWeeksMonth2 = null;
        }
    }

    private void InitializeDisplayMonths()
    {
        if (Value != null)
        {
            _displayMonth1 = new DateTime(Value.Start.Year, Value.Start.Month, 1);
        }
        else
        {
            _displayMonth1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
        _displayMonth2 = _displayMonth1.AddMonths(1);
    }

    private string FormatRange(DateRange range) =>
        $"{range.Start.ToString(DateFormat, CultureInfo.InvariantCulture)} - {range.End.ToString(DateFormat, CultureInfo.InvariantCulture)}";

    private void PreviousMonth()
    {
        _displayMonth1 = _displayMonth1.AddMonths(-1);
        _displayMonth2 = _displayMonth2.AddMonths(-1);
        _cachedWeeksMonth1 = null;
        _cachedWeeksMonth2 = null;
    }

    private void NextMonth()
    {
        _displayMonth1 = _displayMonth1.AddMonths(1);
        _displayMonth2 = _displayMonth2.AddMonths(1);
        _cachedWeeksMonth1 = null;
        _cachedWeeksMonth2 = null;
    }

    private void SelectDate(DateTime date)
    {
        if (IsDateDisabled(date))
        {
            return;
        }

        if (!_selectionStart.HasValue || (_selectionStart.HasValue && _selectionEnd.HasValue))
        {
            // Start new selection
            _selectionStart = date;
            _selectionEnd = null;
        }
        else
        {
            // Complete selection
            _selectionEnd = date;

            // Ensure start is before end
            if (_selectionEnd < _selectionStart)
            {
                (_selectionStart, _selectionEnd) = (_selectionEnd, _selectionStart);
            }

            // Validate range constraints
            var days = (_selectionEnd.Value - _selectionStart.Value).Days + 1;
            if (MinDays.HasValue && days < MinDays.Value)
            {
                // Reset selection
                _selectionStart = date;
                _selectionEnd = null;
                return;
            }
            if (MaxDays.HasValue && days > MaxDays.Value)
            {
                // Reset selection
                _selectionStart = date;
                _selectionEnd = null;
                return;
            }
        }
    }

    private async Task Apply()
    {
        if (_selectionStart.HasValue && _selectionEnd.HasValue)
        {
            var range = DateRange.Create(_selectionStart.Value, _selectionEnd.Value);
            Value = range;
            await ValueChanged.InvokeAsync(range);
            _isOpen = false;
        }
    }

    private async Task Clear()
    {
        _selectionStart = null;
        _selectionEnd = null;
        Value = null;
        await ValueChanged.InvokeAsync(null);
    }

    private void ApplyPreset(DateRangePreset preset)
    {
        var range = GetPresetRange(preset);
        if (range != null && CountSelectedDays(range.Start, range.End) > 0)
        {
            _selectionStart = range.Start;
            _selectionEnd = range.End;
            _displayMonth1 = new DateTime(range.Start.Year, range.Start.Month, 1);
            _displayMonth2 = _displayMonth1.AddMonths(1);
            _cachedWeeksMonth1 = null;
            _cachedWeeksMonth2 = null;
        }
    }

    private static DateRange? GetPresetRange(DateRangePreset preset)
    {
        var today = DateTime.Today;
        return preset switch
        {
            DateRangePreset.Today => new DateRange(today, today),
            DateRangePreset.Yesterday => new DateRange(today.AddDays(-1), today.AddDays(-1)),
            DateRangePreset.Last7Days => new DateRange(today.AddDays(-6), today),
            DateRangePreset.Last30Days => new DateRange(today.AddDays(-29), today),
            DateRangePreset.ThisMonth => new DateRange(new DateTime(today.Year, today.Month, 1),
                                                        new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1)),
            DateRangePreset.LastMonth => new DateRange(new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                                                        new DateTime(today.Year, today.Month, 1).AddDays(-1)),
            DateRangePreset.ThisYear => new DateRange(new DateTime(today.Year, 1, 1),
                                                       new DateTime(today.Year, 12, 31)),
            _ => null
        };
    }

    private static IEnumerable<DateRangePreset> GetAvailablePresets()
    {
        yield return DateRangePreset.Today;
        yield return DateRangePreset.Yesterday;
        yield return DateRangePreset.Last7Days;
        yield return DateRangePreset.Last30Days;
        yield return DateRangePreset.ThisMonth;
        yield return DateRangePreset.LastMonth;
    }

    private static string GetPresetLabel(DateRangePreset preset) => preset switch
    {
        DateRangePreset.Today => "Today",
        DateRangePreset.Yesterday => "Yesterday",
        DateRangePreset.Last7Days => "Last 7 days",
        DateRangePreset.Last30Days => "Last 30 days",
        DateRangePreset.ThisMonth => "This month",
        DateRangePreset.LastMonth => "Last month",
        DateRangePreset.ThisYear => "This year",
        _ => "Custom"
    };

    private bool IsDateDisabled(DateTime date)
    {
        if (MinDate.HasValue && date < MinDate.Value.Date)
        {
            return true;
        }

        if (MaxDate.HasValue && date > MaxDate.Value.Date)
        {
            return true;
        }

        if (DisabledDates != null && DisabledDates(date))
        {
            return true;
        }

        return false;
    }

    private int CountSelectedDays(DateTime start, DateTime end)
    {
        var s = start.Date <= end.Date ? start.Date : end.Date;
        var e = start.Date > end.Date ? start.Date : end.Date;

        // Fast path: no disabled dates callback — use O(1) arithmetic
        if (DisabledDates == null && !MinDate.HasValue && !MaxDate.HasValue)
        {
            return (e - s).Days + 1;
        }

        var count = 0;
        var current = s;
        while (current <= e)
        {
            if (!IsDateDisabled(current))
            {
                count++;
            }
            current = current.AddDays(1);
        }
        return count;
    }

    private bool IsInRange(DateTime date)
    {
        if (!_selectionStart.HasValue)
        {
            return false;
        }

        if (_selectionEnd.HasValue)
        {
            var start = _selectionStart.Value < _selectionEnd.Value ? _selectionStart.Value : _selectionEnd.Value;
            var end = _selectionStart.Value > _selectionEnd.Value ? _selectionStart.Value : _selectionEnd.Value;
            return date.Date >= start.Date && date.Date <= end.Date;
        }

        return date.Date == _selectionStart.Value.Date;
    }

    private bool IsRangeStart(DateTime date)
    {
        if (!_selectionStart.HasValue)
        {
            return false;
        }

        if (!_selectionEnd.HasValue)
        {
            return date.Date == _selectionStart.Value.Date;
        }

        var start = _selectionStart.Value < _selectionEnd.Value ? _selectionStart.Value : _selectionEnd.Value;
        return date.Date == start.Date;
    }

    private bool IsRangeEnd(DateTime date)
    {
        if (!_selectionEnd.HasValue)
        {
            return false;
        }

        var end = _selectionStart!.Value > _selectionEnd.Value ? _selectionStart.Value : _selectionEnd.Value;
        return date.Date == end.Date;
    }

    private List<List<DateTime?>> GetWeeksInMonth1() =>
        _cachedWeeksMonth1 ??= BuildWeeksInMonth(_displayMonth1);

    private List<List<DateTime?>> GetWeeksInMonth2() =>
        _cachedWeeksMonth2 ??= BuildWeeksInMonth(_displayMonth2);

    private List<List<DateTime?>> BuildWeeksInMonth(DateTime monthDate)
    {
        var weeks = new List<List<DateTime?>>();
        var firstOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var startDate = firstOfMonth;
        while (startDate.DayOfWeek != FirstDayOfWeek)
        {
            startDate = startDate.AddDays(-1);
        }

        var currentDate = startDate;
        while (currentDate <= lastOfMonth || weeks.Count < 6)
        {
            var week = new List<DateTime?>();
            for (var i = 0; i < 7; i++)
            {
                if (currentDate.Month == monthDate.Month)
                {
                    week.Add(currentDate);
                }
                else
                {
                    week.Add(null);
                }
                currentDate = currentDate.AddDays(1);
            }
            weeks.Add(week);

            if (currentDate.Month != monthDate.Month && weeks.Count >= 5)
            {
                break;
            }
        }

        return weeks;
    }

    private static string GetMonthName(int month) => MonthNames[month - 1];

    private string ButtonCssClass => ClassNames.cn(
        ShowTwoMonths ? "w-[300px]" : "w-[240px]",
        "justify-start text-left font-normal",
        Value == null ? "text-muted-foreground" : null,
        Disabled ? "opacity-50 pointer-events-none" : null,
        Class
    );

    private string GetPresetButtonClass(DateRangePreset preset)
    {
        var range = GetPresetRange(preset);
        var isSelected = range != null && _selectionStart.HasValue && _selectionEnd.HasValue &&
                         range.Start == _selectionStart.Value && range.End == _selectionEnd.Value;

        return ClassNames.cn(
            "text-left px-2 py-1.5 text-sm rounded-md transition-colors whitespace-nowrap shrink-0",
            isSelected ? "bg-primary text-primary-foreground" : "hover:bg-accent hover:text-accent-foreground"
        );
    }

    private static string GetCellClass(DateTime? day, bool isInRange, bool isRangeStart, bool isRangeEnd)
    {
        if (!day.HasValue)
        {
            return CellEmpty;
        }

        if (isRangeStart)
        {
            return CellRangeStart;
        }

        if (isRangeEnd)
        {
            return CellRangeEnd;
        }

        if (isInRange)
        {
            return CellInRange;
        }

        return CellDefault;
    }

    private static string GetDayClass(DateTime date, bool isDisabled, bool isInRange, bool isRangeStart, bool isRangeEnd, bool isToday)
    {
        if (isDisabled)
        {
            return DayDisabled;
        }

        if (isRangeStart || isRangeEnd)
        {
            return DayRangeEndpoint;
        }

        if (isInRange)
        {
            return DayInRange;
        }

        if (isToday)
        {
            return DayToday;
        }

        return DayDefault;
    }

    /// <summary>
    /// Determines whether the component should re-render based on tracked state changes.
    /// This optimization reduces unnecessary render cycles.
    /// </summary>
    protected override bool ShouldRender()
    {
        var changed = _lastIsOpen != _isOpen
            || _lastDisplayMonth1 != _displayMonth1
            || _lastDisplayMonth2 != _displayMonth2
            || _lastSelectionStart != _selectionStart
            || _lastSelectionEnd != _selectionEnd
            || _lastValue != Value
            || _lastMinDate != MinDate
            || _lastMaxDate != MaxDate
            || _lastShowTwoMonths != ShowTwoMonths
            || _lastShowPresets != ShowPresets
            || _lastDisabled != Disabled;

        if (changed)
        {
            _lastIsOpen = _isOpen;
            _lastDisplayMonth1 = _displayMonth1;
            _lastDisplayMonth2 = _displayMonth2;
            _lastSelectionStart = _selectionStart;
            _lastSelectionEnd = _selectionEnd;
            _lastValue = Value;
            _lastMinDate = MinDate;
            _lastMaxDate = MaxDate;
            _lastShowTwoMonths = ShowTwoMonths;
            _lastShowPresets = ShowPresets;
            _lastDisabled = Disabled;
            return true;
        }

        return false;
    }
}
