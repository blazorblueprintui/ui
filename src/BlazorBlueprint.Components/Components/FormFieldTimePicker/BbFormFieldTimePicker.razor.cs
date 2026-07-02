using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbTimePicker"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldTimePicker provides a higher-level abstraction over <see cref="BbTimePicker"/>.
/// It automatically handles label display, helper text, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbTimePicker"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbFormFieldTimePicker @bind-Value="meetingTime"
///                        Label="Meeting Time"
///                        HelperText="Business hours only"
///                        Format="TimeFormat.Hour24" /&gt;
/// </code>
/// </example>
public partial class BbFormFieldTimePicker : FormFieldBase
{
    // --- TimePicker Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the selected time value.
    /// </summary>
    [Parameter]
    public TimeSpan? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected time changes.
    /// </summary>
    [Parameter]
    public EventCallback<TimeSpan?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<TimeSpan?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the time format (12-hour or 24-hour).
    /// </summary>
    [Parameter]
    public TimeFormat Format { get; set; } = TimeFormat.Hour12;

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
    /// Gets or sets the minimum allowed time.
    /// </summary>
    [Parameter]
    public TimeSpan? MinTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed time.
    /// </summary>
    [Parameter]
    public TimeSpan? MaxTime { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text when no time is selected.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select time";

    /// <summary>
    /// Gets or sets whether the time picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the time picker is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner TimePicker trigger button.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TimeSpan? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
