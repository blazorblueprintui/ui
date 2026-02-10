using System.Globalization;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.DateRangePicker;

/// <summary>
/// A date range picker component with two-month calendar view.
/// </summary>
public partial class DateRangePicker : ComponentBase
{
    private bool _isOpen;
    private DateTime _displayMonth1;
    private DateTime _displayMonth2;
    private DateTime? _selectionStart;
    private DateTime? _selectionEnd;

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

    private string[] DayNames
    {
        get
        {
            var start = (int)FirstDayOfWeek;
            var result = new string[7];
            for (var i = 0; i < 7; i++)
            {
                result[i] = BaseDayNames[(start + i) % 7];
            }
            return result;
        }
    }
    private static readonly string[] MonthNames = { "January", "February", "March", "April", "May", "June",
                                                     "July", "August", "September", "October", "November", "December" };

    private bool CanApply => _selectionStart.HasValue && _selectionEnd.HasValue;

    protected override void OnInitialized()
    {
        InitializeDisplayMonths();
        if (Value != null)
        {
            _selectionStart = Value.Start;
            _selectionEnd = Value.End;
        }
    }

    protected override void OnParametersSet()
    {
        if (Value != null)
        {
            _selectionStart = Value.Start;
            _selectionEnd = Value.End;
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
        StateHasChanged();
    }

    private void NextMonth()
    {
        _displayMonth1 = _displayMonth1.AddMonths(1);
        _displayMonth2 = _displayMonth2.AddMonths(1);
        StateHasChanged();
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
                StateHasChanged();
                return;
            }
            if (MaxDays.HasValue && days > MaxDays.Value)
            {
                // Reset selection
                _selectionStart = date;
                _selectionEnd = null;
                StateHasChanged();
                return;
            }
        }
        StateHasChanged();
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
            StateHasChanged();
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
        var count = 0;
        var current = start.Date <= end.Date ? start.Date : end.Date;
        var last = start.Date > end.Date ? start.Date : end.Date;
        while (current <= last)
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

    private List<List<DateTime?>> GetWeeksInMonth(DateTime monthDate)
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
            return "h-9 w-9 flex-1 text-center text-sm p-0";
        }

        return ClassNames.cn(
            "h-9 w-9 flex-1 text-center text-sm p-0 relative",
            isInRange && !isRangeStart && !isRangeEnd ? "bg-accent" : null,
            isRangeStart ? "rounded-l-md bg-accent" : null,
            isRangeEnd ? "rounded-r-md bg-accent" : null
        );
    }

    private static string GetDayClass(DateTime date, bool isDisabled, bool isInRange, bool isRangeStart, bool isRangeEnd, bool isToday)
    {
        return ClassNames.cn(
            "inline-flex h-9 w-full items-center justify-center rounded-md text-sm font-normal",
            "ring-offset-background transition-colors",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
            "disabled:pointer-events-none disabled:opacity-50",
            isDisabled ? "text-muted-foreground opacity-50" : null,
            (isRangeStart || isRangeEnd) ? "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground" : null,
            isInRange && !isRangeStart && !isRangeEnd ? "bg-accent text-accent-foreground" : null,
            !isInRange && !isDisabled ? "hover:bg-accent hover:text-accent-foreground" : null,
            isToday && !isRangeStart && !isRangeEnd ? "bg-accent text-accent-foreground" : null
        );
    }
}
