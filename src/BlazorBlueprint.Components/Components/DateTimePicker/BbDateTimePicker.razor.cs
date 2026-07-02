using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

/// <summary>
/// A date and time picker component that combines a popover calendar with
/// hour/minute (and optional second / AM-PM) selectors.
/// </summary>
/// <remarks>
/// <para>
/// Selecting a date preserves the current time portion, and changing the time preserves
/// the selected date (defaulting to today when no date has been picked yet). The popover
/// stays open after a date is selected so the time can be adjusted; it closes when the
/// user clicks outside or presses Escape.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDateTimePicker @bind-Value="appointment" /&gt;
/// </code>
/// </example>
public partial class BbDateTimePicker : ComponentBase
{
    private bool _isOpen;
    private BbCalendar? _calendar;
    private bool _focusDone;
    private int _internalHour;
    private int _internalMinute;
    private int _internalSecond;
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;

    [Inject]
    private IBbLocalizer Localizer { get; set; } = default!;

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// The selected date and time value.
    /// </summary>
    [Parameter]
    public DateTime? Value { get; set; }

    /// <summary>
    /// Callback when the selected date and time changes.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<DateTime?>>? ValueExpression { get; set; }

    /// <summary>
    /// The date and time format to display on the trigger button.
    /// When null, defaults to "G" (culture-aware date and time with seconds) if
    /// <see cref="ShowSeconds"/> is true, otherwise "g" (culture-aware date and time).
    /// </summary>
    [Parameter]
    public string? DateTimeFormat { get; set; }

    /// <summary>
    /// Placeholder text when no date and time is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

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
    /// Function returning additional CSS classes for a specific day button in the
    /// calendar, composed with the built-in day classes.
    /// </summary>
    [Parameter]
    public Func<DateTime, string?>? DayClassFunc { get; set; }

    /// <summary>
    /// Optional template for rendering custom content inside each calendar day button.
    /// When null, the day number is rendered.
    /// </summary>
    [Parameter]
    public RenderFragment<CalendarDayContext>? DayTemplate { get; set; }

    /// <summary>
    /// The first day of the week. Defaults to the current culture's first day of week.
    /// </summary>
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The time format for the time selectors (12-hour with AM/PM or 24-hour).
    /// </summary>
    [Parameter]
    public TimeFormat TimeFormat { get; set; } = TimeFormat.Hour12;

    /// <summary>
    /// Whether to show the seconds selector.
    /// </summary>
    [Parameter]
    public bool ShowSeconds { get; set; }

    /// <summary>
    /// The minute step interval.
    /// </summary>
    [Parameter]
    public int MinuteStep { get; set; } = 1;

    /// <summary>
    /// Whether the date time picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the date time picker is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the trigger button.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional attributes to apply to the trigger button.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string EffectivePlaceholder => Placeholder ?? Localizer["DateTimePicker.Placeholder"];

    private string EffectiveFormat => DateTimeFormat ?? (ShowSeconds ? "G" : "g");

    private int CurrentHour => Value?.Hour ?? _internalHour;

    private int CurrentMinute => Value?.Minute ?? _internalMinute;

    private int CurrentSecond => Value?.Second ?? _internalSecond;

    private bool IsAm => CurrentHour < 12;

    private int DisplayHour
    {
        get
        {
            if (TimeFormat == TimeFormat.Hour24)
            {
                return CurrentHour;
            }

            var hour = CurrentHour % 12;
            return hour == 0 ? 12 : hour;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Value.HasValue)
        {
            _internalHour = Value.Value.Hour;
            _internalMinute = Value.Value.Minute;
            _internalSecond = Value.Value.Second;
        }

        if (CascadedEditContext != null && ValueExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    private async Task HandleDateSelected(DateTime? date)
    {
        if (date.HasValue)
        {
            // Keep the current time portion when the date changes; the popover
            // stays open so the user can adjust the time.
            await SetValue(date.Value.Date.AddHours(_internalHour).AddMinutes(_internalMinute).AddSeconds(_internalSecond));
        }
        else
        {
            await SetValue(null);
        }
    }

    private void HandleOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;

        // Re-arm focus-on-open every time the popover opens, so the active day is
        // focused again on the next open regardless of how the previous one closed.
        if (isOpen)
        {
            _focusDone = false;
        }
    }

    private async Task HandleContentReady()
    {
        // Move focus into the calendar once the popover is positioned, so the active
        // day is focused and arrow-key navigation works immediately on open. Guard so
        // a repositioning re-fire doesn't yank focus back from where the user navigated.
        if (_focusDone || _calendar is null)
        {
            return;
        }

        _focusDone = true;
        await _calendar.FocusActiveDayAsync();
    }

    private async Task SetValue(DateTime? value)
    {
        Value = value;

        if (value.HasValue)
        {
            _internalHour = value.Value.Hour;
            _internalMinute = value.Value.Minute;
            _internalSecond = value.Value.Second;
        }

        await ValueChanged.InvokeAsync(value);

        if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    private async Task UpdateTime()
    {
        // Changing the time keeps the selected date; when no date has been
        // picked yet, default to today.
        var date = Value?.Date ?? DateTime.Today;
        await SetValue(date.AddHours(_internalHour).AddMinutes(_internalMinute).AddSeconds(_internalSecond));
    }

    private async Task IncrementHour()
    {
        if (TimeFormat == TimeFormat.Hour24)
        {
            _internalHour = (_internalHour + 1) % 24;
        }
        else
        {
            // Cycle within the current AM/PM period, preserving the period.
            var isAm = _internalHour < 12;
            var hour12 = (_internalHour % 12 + 1) % 12;
            _internalHour = isAm ? hour12 : hour12 + 12;
        }

        await UpdateTime();
    }

    private async Task DecrementHour()
    {
        if (TimeFormat == TimeFormat.Hour24)
        {
            _internalHour = (_internalHour - 1 + 24) % 24;
        }
        else
        {
            // Cycle within the current AM/PM period, preserving the period.
            var isAm = _internalHour < 12;
            var hour12 = (_internalHour % 12 - 1 + 12) % 12;
            _internalHour = isAm ? hour12 : hour12 + 12;
        }

        await UpdateTime();
    }

    private async Task IncrementMinute()
    {
        _internalMinute = (_internalMinute + MinuteStep) % 60;
        await UpdateTime();
    }

    private async Task DecrementMinute()
    {
        _internalMinute = (_internalMinute - MinuteStep + 60) % 60;
        await UpdateTime();
    }

    private async Task IncrementSecond()
    {
        _internalSecond = (_internalSecond + 1) % 60;
        await UpdateTime();
    }

    private async Task DecrementSecond()
    {
        _internalSecond = (_internalSecond - 1 + 60) % 60;
        await UpdateTime();
    }

    private async Task SetAmPm(bool isAm)
    {
        if (isAm && _internalHour >= 12)
        {
            _internalHour -= 12;
        }
        else if (!isAm && _internalHour < 12)
        {
            _internalHour += 12;
        }

        await UpdateTime();
    }

    private async Task SetNow()
    {
        var now = DateTime.Now;
        _internalHour = now.Hour;
        _internalMinute = now.Minute / MinuteStep * MinuteStep;
        _internalSecond = ShowSeconds ? now.Second : 0;
        await SetValue(now.Date.AddHours(_internalHour).AddMinutes(_internalMinute).AddSeconds(_internalSecond));
    }

    private async Task Clear()
    {
        _internalHour = 0;
        _internalMinute = 0;
        _internalSecond = 0;
        await SetValue(null);
    }

    /// <summary>
    /// Gets the computed CSS classes for the trigger button.
    /// </summary>
    private string ButtonCssClass => ClassNames.cn(
        "w-[280px] justify-start text-left font-normal",
        !Value.HasValue ? "text-muted-foreground" : null,
        Disabled ? "opacity-50 pointer-events-none" : null,
        Class
    );

    private static string ScrollButtonClass => ClassNames.cn(
        "w-12 h-8 flex items-center justify-center",
        "hover:bg-accent hover:text-accent-foreground",
        "transition-colors",
        "disabled:opacity-50 disabled:cursor-not-allowed"
    );

    private string GetAmPmButtonClass(bool isAm) => ClassNames.cn(
        "w-12 h-10 text-sm font-medium transition-colors",
        isAm == IsAm ? "bg-primary text-primary-foreground" : "hover:bg-accent hover:text-accent-foreground",
        "disabled:opacity-50 disabled:cursor-not-allowed"
    );
}
