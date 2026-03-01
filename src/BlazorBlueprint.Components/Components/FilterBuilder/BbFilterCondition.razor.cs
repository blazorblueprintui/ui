using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a single filter condition row: field selector, operator selector, and type-aware value input.
/// </summary>
public partial class BbFilterCondition : ComponentBase
{
    private FilterField? selectedField;
    private IEnumerable<SelectOption<string>>? fieldOptions;
    private IEnumerable<SelectOption<FilterOperator>>? operatorOptions;

    private static readonly IEnumerable<SelectOption<InLastPeriod>> inLastPeriodOptions = new[]
    {
        new SelectOption<InLastPeriod>(InLastPeriod.Days, "days"),
        new SelectOption<InLastPeriod>(InLastPeriod.Weeks, "weeks"),
        new SelectOption<InLastPeriod>(InLastPeriod.Months, "months")
    };

    /// <summary>
    /// Gets or sets the condition data for this row.
    /// </summary>
    [Parameter]
    public FilterCondition Condition { get; set; } = null!;

    /// <summary>
    /// Gets or sets the callback invoked when this condition should be removed.
    /// </summary>
    [Parameter]
    public EventCallback OnRemove { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when any part of this condition changes.
    /// </summary>
    [Parameter]
    public EventCallback OnChanged { get; set; }

    /// <summary>
    /// Gets the cascaded filter builder context.
    /// </summary>
    [CascadingParameter]
    private FilterBuilderContext? Context { get; set; }

    protected override void OnParametersSet()
    {
        if (Context != null)
        {
            fieldOptions = Context.Fields.Select(f => new SelectOption<string>(f.Name, f.Label));
            UpdateSelectedField();
        }
    }

    private void UpdateSelectedField()
    {
        if (Context == null || string.IsNullOrEmpty(Condition.Field))
        {
            selectedField = null;
            operatorOptions = null;
            return;
        }

        selectedField = Context.Fields.FirstOrDefault(f => string.Equals(f.Name, Condition.Field, StringComparison.OrdinalIgnoreCase));
        if (selectedField != null)
        {
            operatorOptions = FilterOperatorHelper.GetOperatorOptions(selectedField.Type);
        }
    }

    private async Task HandleFieldChanged(string? newField)
    {
        var previousFieldType = selectedField?.Type;
        Condition.Field = newField ?? "";
        UpdateSelectedField();

        if (selectedField != null && selectedField.Type != previousFieldType)
        {
            var operators = FilterOperatorHelper.GetOperatorsForType(selectedField.Type);
            Condition.Operator = operators.Length > 0 ? operators[0] : default;
            Condition.Value = null;
            Condition.ValueEnd = null;
        }

        await OnChanged.InvokeAsync();
    }

    private async Task HandleOperatorChanged(FilterOperator newOperator)
    {
        var previousOperator = Condition.Operator;
        Condition.Operator = newOperator;

        if (FilterOperatorHelper.IsValuelessOperator(Condition.Operator))
        {
            Condition.Value = null;
            Condition.ValueEnd = null;
        }
        else if (FilterOperatorHelper.IsRangeOperator(Condition.Operator) != FilterOperatorHelper.IsRangeOperator(previousOperator))
        {
            Condition.Value = null;
            Condition.ValueEnd = null;
        }

        await OnChanged.InvokeAsync();
    }

    private async Task HandleRemove() =>
        await OnRemove.InvokeAsync();

    // String value helpers
    private string GetStringValue() => Condition.Value as string ?? "";

    private async Task HandleStringValueChanged(string? value)
    {
        Condition.Value = value;
        await OnChanged.InvokeAsync();
    }

    // Double value helpers
    private double GetDoubleValue()
    {
        return Condition.Value switch
        {
            double d => d,
            int i => i,
            float f => f,
            decimal m => (double)m,
            _ => 0
        };
    }

    private double GetDoubleValueEnd()
    {
        return Condition.ValueEnd switch
        {
            double d => d,
            int i => i,
            float f => f,
            decimal m => (double)m,
            _ => 0
        };
    }

    private async Task HandleDoubleValueChanged(double value)
    {
        Condition.Value = value;
        await OnChanged.InvokeAsync();
    }

    private async Task HandleDoubleValueEndChanged(double value)
    {
        Condition.ValueEnd = value;
        await OnChanged.InvokeAsync();
    }

    // DateTime value helpers
    private DateTime? GetDateTimeValue()
    {
        return Condition.Value switch
        {
            DateTime dt => dt,
            _ => null
        };
    }

    private async Task HandleDateTimeValueChanged(DateTime? value)
    {
        Condition.Value = value;
        await OnChanged.InvokeAsync();
    }

    // DateRange value helpers
    private DateRange? GetDateRangeValue()
    {
        if (Condition.Value is DateTime start && Condition.ValueEnd is DateTime end)
        {
            return new DateRange(start, end);
        }
        return null;
    }

    private async Task HandleDateRangeValueChanged(DateRange? range)
    {
        if (range != null)
        {
            Condition.Value = range.Start;
            Condition.ValueEnd = range.End;
        }
        else
        {
            Condition.Value = null;
            Condition.ValueEnd = null;
        }
        await OnChanged.InvokeAsync();
    }

    // InLast helpers — Value stores the amount (int), ValueEnd stores the period (InLastPeriod)
    private int GetInLastAmount()
    {
        return Condition.Value switch
        {
            int i => i,
            double d => (int)d,
            _ => 0
        };
    }

    private InLastPeriod GetInLastPeriod()
    {
        return Condition.ValueEnd switch
        {
            InLastPeriod p => p,
            int i when System.Enum.IsDefined(typeof(InLastPeriod), i) => (InLastPeriod)i,
            string s when System.Enum.TryParse<InLastPeriod>(s, out var p) => p,
            _ => InLastPeriod.Days
        };
    }

    private async Task HandleInLastAmountChanged(int value)
    {
        Condition.Value = value;
        await OnChanged.InvokeAsync();
    }

    private async Task HandleInLastPeriodChanged(InLastPeriod value)
    {
        Condition.ValueEnd = value;
        await OnChanged.InvokeAsync();
    }

    // MultiSelect helpers
    private IEnumerable<string>? GetMultiSelectValues()
    {
        return Condition.Value switch
        {
            IEnumerable<string> values => values,
            _ => null
        };
    }

    private async Task HandleMultiSelectValuesChanged(IEnumerable<string>? values)
    {
        Condition.Value = values?.ToArray();
        await OnChanged.InvokeAsync();
    }

    // CSS classes
    private string RowCssClass => ClassNames.cn(
        "flex items-center gap-2",
        Context?.Compact == true ? "gap-1" : "gap-2"
    );

    private string FieldSelectClass => Context?.Compact == true ? "w-[140px]" : "w-[160px]";

    private string OperatorSelectClass => Context?.Compact == true ? "w-[160px]" : "w-[180px]";

    private string ValueInputClass => Context?.Compact == true ? "w-[140px]" : "w-[180px]";
}
