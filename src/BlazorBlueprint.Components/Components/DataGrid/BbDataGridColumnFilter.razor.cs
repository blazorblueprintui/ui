using BlazorBlueprint.Primitives.Filtering;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a per-column filter UI inside a popover. Displays an operator dropdown
/// and type-aware value input with Apply and Clear buttons.
/// </summary>
public partial class BbDataGridColumnFilter : ComponentBase
{
    private FilterCondition editCondition = new();
    private IEnumerable<SelectOption<FilterOperator>>? operatorOptions;
    private bool hasInitializedEditCondition;
    private FilterCondition? trackedCurrentFilter;

    [Inject] private IBbLocalizer Localizer { get; set; } = default!;

    private IEnumerable<SelectOption<InLastPeriod>> InLastPeriodOptions => new[]
    {
        new SelectOption<InLastPeriod>(InLastPeriod.Days, Localizer["FilterBuilder.Days"]),
        new SelectOption<InLastPeriod>(InLastPeriod.Weeks, Localizer["FilterBuilder.Weeks"]),
        new SelectOption<InLastPeriod>(InLastPeriod.Months, Localizer["FilterBuilder.Months"]),
        new SelectOption<InLastPeriod>(InLastPeriod.Hours, Localizer["FilterBuilder.Hours"]),
        new SelectOption<InLastPeriod>(InLastPeriod.Minutes, Localizer["FilterBuilder.Minutes"]),
        new SelectOption<InLastPeriod>(InLastPeriod.Seconds, Localizer["FilterBuilder.Seconds"])
    };

    private IEnumerable<SelectOption<DatePreset>> DatePresetOptions => new[]
    {
        new SelectOption<DatePreset>(DatePreset.Today, Localizer["FilterBuilder.Today"]),
        new SelectOption<DatePreset>(DatePreset.Yesterday, Localizer["FilterBuilder.Yesterday"]),
        new SelectOption<DatePreset>(DatePreset.Tomorrow, Localizer["FilterBuilder.Tomorrow"]),
        new SelectOption<DatePreset>(DatePreset.ThisWeek, Localizer["FilterBuilder.ThisWeek"]),
        new SelectOption<DatePreset>(DatePreset.LastWeek, Localizer["FilterBuilder.LastWeek"]),
        new SelectOption<DatePreset>(DatePreset.NextWeek, Localizer["FilterBuilder.NextWeek"]),
        new SelectOption<DatePreset>(DatePreset.ThisMonth, Localizer["FilterBuilder.ThisMonth"]),
        new SelectOption<DatePreset>(DatePreset.LastMonth, Localizer["FilterBuilder.LastMonth"]),
        new SelectOption<DatePreset>(DatePreset.NextMonth, Localizer["FilterBuilder.NextMonth"]),
        new SelectOption<DatePreset>(DatePreset.ThisQuarter, Localizer["FilterBuilder.ThisQuarter"]),
        new SelectOption<DatePreset>(DatePreset.LastQuarter, Localizer["FilterBuilder.LastQuarter"]),
        new SelectOption<DatePreset>(DatePreset.ThisYear, Localizer["FilterBuilder.ThisYear"]),
        new SelectOption<DatePreset>(DatePreset.LastYear, Localizer["FilterBuilder.LastYear"])
    };

    /// <summary>
    /// The column title for the filter header label.
    /// </summary>
    [Parameter]
    public string? ColumnTitle { get; set; }

    /// <summary>
    /// The filter field type that determines available operators and value inputs.
    /// </summary>
    [Parameter]
    public FilterFieldType FieldType { get; set; }

    /// <summary>
    /// The field name used in the FilterCondition.Field property.
    /// </summary>
    [Parameter]
    public string FieldName { get; set; } = "";

    /// <summary>
    /// Predefined options for Enum filter fields.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<string>>? FilterOptions { get; set; }

    /// <summary>
    /// The current active filter condition, or null if no filter is active.
    /// </summary>
    [Parameter]
    public FilterCondition? CurrentFilter { get; set; }

    /// <summary>
    /// Callback invoked when the filter is applied. Passes the new condition.
    /// </summary>
    [Parameter]
    public EventCallback<FilterCondition> OnApply { get; set; }

    /// <summary>
    /// Callback invoked when the filter is cleared.
    /// </summary>
    [Parameter]
    public EventCallback OnClear { get; set; }

    protected override void OnParametersSet()
    {
        operatorOptions = FilterOperatorHelper.GetOperatorOptions(FieldType);

        // Only reset the edit condition when the external CurrentFilter actually changes.
        // Re-renders caused by portal registrations (e.g. BbSelect opening its dropdown)
        // must NOT discard the user's in-progress edits.
        if (!hasInitializedEditCondition || !ReferenceEquals(CurrentFilter, trackedCurrentFilter))
        {
            hasInitializedEditCondition = true;
            trackedCurrentFilter = CurrentFilter;

            if (CurrentFilter != null)
            {
                editCondition = CurrentFilter.Clone();
            }
            else
            {
                ResetEditCondition();
            }
        }
    }

    private void ResetEditCondition()
    {
        var operators = FilterOperatorHelper.GetOperatorsForType(FieldType);
        editCondition = new FilterCondition
        {
            Field = FieldName,
            Operator = operators.Length > 0 ? operators[0] : default
        };
    }

    private void HandleOperatorChanged(FilterOperator newOperator)
    {
        var previousOperator = editCondition.Operator;
        editCondition.Operator = newOperator;

        if (FilterOperatorHelper.IsValuelessOperator(editCondition.Operator))
        {
            editCondition.Value = null;
            editCondition.ValueEnd = null;
        }
        else if (FilterOperatorHelper.IsRangeOperator(editCondition.Operator) != FilterOperatorHelper.IsRangeOperator(previousOperator))
        {
            editCondition.Value = null;
            editCondition.ValueEnd = null;
        }
        else if (FilterOperatorHelper.IsDatePresetOperator(editCondition.Operator) != FilterOperatorHelper.IsDatePresetOperator(previousOperator))
        {
            editCondition.Value = FilterOperatorHelper.IsDatePresetOperator(editCondition.Operator) ? DatePreset.Today : null;
            editCondition.ValueEnd = null;
        }
    }

    private async Task HandleApply()
    {
        editCondition.Field = FieldName;
        await OnApply.InvokeAsync(editCondition.Clone());
    }

    private async Task HandleClear()
    {
        ResetEditCondition();
        await OnClear.InvokeAsync();
    }

    // String value helpers
    private string GetStringValue() => editCondition.Value as string ?? "";

    private void HandleStringValueChanged(string? value) => editCondition.Value = value;

    // Double value helpers
    private double GetDoubleValue() => editCondition.Value switch
    {
        double d => d,
        int i => i,
        float f => f,
        decimal m => (double)m,
        _ => 0
    };

    private double GetDoubleValueEnd() => editCondition.ValueEnd switch
    {
        double d => d,
        int i => i,
        float f => f,
        decimal m => (double)m,
        _ => 0
    };

    private void HandleDoubleValueChanged(double value) => editCondition.Value = value;

    private void HandleDoubleValueEndChanged(double value) => editCondition.ValueEnd = value;

    // DateTime value helpers
    private DateTime? GetDateTimeValue() => editCondition.Value switch
    {
        DateTime dt => dt,
        _ => null
    };

    private void HandleDateTimeValueChanged(DateTime? value) => editCondition.Value = value;

    // DateRange value helpers
    private DateRange? GetDateRangeValue()
    {
        if (editCondition.Value is DateTime start && editCondition.ValueEnd is DateTime end)
        {
            return new DateRange(start, end);
        }
        return null;
    }

    private void HandleDateRangeValueChanged(DateRange? range)
    {
        if (range != null)
        {
            editCondition.Value = range.Start;
            editCondition.ValueEnd = range.End;
        }
        else
        {
            editCondition.Value = null;
            editCondition.ValueEnd = null;
        }
    }

    // DatePreset helpers
    private DatePreset GetDatePreset() => editCondition.Value switch
    {
        DatePreset p => p,
        int i when Enum.IsDefined(typeof(DatePreset), i) => (DatePreset)i,
        string s when Enum.TryParse<DatePreset>(s, out var p) => p,
        _ => DatePreset.Today
    };

    private void HandleDatePresetChanged(DatePreset value) => editCondition.Value = value;

    // InLast helpers
    private int GetInLastAmount() => editCondition.Value switch
    {
        int i => i,
        double d => (int)d,
        _ => 0
    };

    private InLastPeriod GetInLastPeriod() => editCondition.ValueEnd switch
    {
        InLastPeriod p => p,
        int i when Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
        string s when Enum.TryParse<InLastPeriod>(s, out var p) => p,
        _ => InLastPeriod.Days
    };

    private void HandleInLastAmountChanged(int value) => editCondition.Value = value;

    private void HandleInLastPeriodChanged(InLastPeriod value) => editCondition.ValueEnd = value;

    // MultiSelect helpers
    private IEnumerable<string>? GetMultiSelectValues() => editCondition.Value switch
    {
        IEnumerable<string> values => values,
        _ => null
    };

    private void HandleMultiSelectValuesChanged(IEnumerable<string>? values) => editCondition.Value = values?.ToArray();
}
