using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbDatePicker"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldDatePicker provides a higher-level abstraction over <see cref="BbDatePicker"/>.
/// It automatically handles label display, helper text, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbDatePicker"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbFormFieldDatePicker @bind-Value="startDate"
///                        Label="Start Date"
///                        HelperText="Select a date in the future"
///                        MinDate="DateTime.Today" /&gt;
/// </code>
/// </example>
public partial class BbFormFieldDatePicker : FormFieldBase
{
    // --- DatePicker Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the selected date value.
    /// </summary>
    [Parameter]
    public DateTime? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected date changes.
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
    /// Gets or sets the date format to display.
    /// Default is "d" (culture-aware short date pattern).
    /// </summary>
    [Parameter]
    public string DateFormat { get; set; } = "d";

    /// <summary>
    /// Gets or sets the placeholder text when no date is selected.
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
    /// Gets or sets whether the date picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the date picker is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner DatePicker trigger button.
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
