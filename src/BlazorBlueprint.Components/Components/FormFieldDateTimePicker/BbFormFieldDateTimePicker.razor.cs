using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbDateTimePicker"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldDateTimePicker provides a higher-level abstraction over <see cref="BbDateTimePicker"/>.
/// It automatically handles label display, helper text, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbDateTimePicker"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbFormFieldDateTimePicker @bind-Value="appointment"
///                            Label="Appointment"
///                            HelperText="Select a date and time"
///                            MinDate="DateTime.Today" /&gt;
/// </code>
/// </example>
public partial class BbFormFieldDateTimePicker : FormFieldBase
{
    // --- DateTimePicker Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the selected date and time value.
    /// </summary>
    [Parameter]
    public DateTime? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected date and time changes.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<DateTime?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the date and time format to display.
    /// When null, defaults to "G" (culture-aware date and time with seconds) if
    /// <see cref="ShowSeconds"/> is true, otherwise "g" (culture-aware date and time).
    /// </summary>
    [Parameter]
    public string? DateTimeFormat { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text when no date and time is selected.
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
    /// Gets or sets the first day of the week.
    /// Defaults to the current culture's first day of week.
    /// </summary>
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the time format for the time selectors (12-hour with AM/PM or 24-hour).
    /// </summary>
    [Parameter]
    public TimeFormat TimeFormat { get; set; } = TimeFormat.Hour12;

    /// <summary>
    /// Gets or sets whether to show the seconds selector.
    /// </summary>
    [Parameter]
    public bool ShowSeconds { get; set; }

    /// <summary>
    /// Gets or sets the minute step interval.
    /// </summary>
    [Parameter]
    public int MinuteStep { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the date time picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the date time picker is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner DateTimePicker trigger button.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(DateTime? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
