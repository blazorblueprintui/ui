using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbDateRangePicker"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldDateRangePicker : FormFieldBase
{
    /// <summary>
    /// Gets or sets the selected date range.
    /// </summary>
    [Parameter]
    public DateRange? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the date range changes.
    /// </summary>
    [Parameter]
    public EventCallback<DateRange?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the date format to display.
    /// </summary>
    [Parameter]
    public string DateFormat { get; set; } = "d";

    /// <summary>
    /// Gets or sets the placeholder text when no range is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the minimum selectable date.
    /// </summary>
    [Parameter]
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum selectable date.
    /// </summary>
    [Parameter]
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Gets or sets a function to determine if a date is disabled.
    /// </summary>
    [Parameter]
    public Func<DateTime, bool>? DisabledDates { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of days that must be selected.
    /// </summary>
    [Parameter]
    public int? MinDays { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of days that can be selected.
    /// </summary>
    [Parameter]
    public int? MaxDays { get; set; }

    /// <summary>
    /// Gets or sets whether to show two months side by side.
    /// </summary>
    [Parameter]
    public bool ShowTwoMonths { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show preset date ranges.
    /// </summary>
    [Parameter]
    public bool ShowPresets { get; set; } = true;

    /// <summary>
    /// Gets or sets custom quick-pick presets.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DateRangeQuickPick>? Presets { get; set; }

    /// <summary>
    /// Gets or sets the first day of the week.
    /// </summary>
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets whether the date range picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner DateRangePicker trigger.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => null;

    private async Task HandleValueChanged(DateRange? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
