using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>A reusable filter. The builder clones its definition before editing.</summary>
public sealed record FilterPreset(string Id, string Label, FilterDefinition Filter);

/// <summary>How saved filter choices are presented.</summary>
public enum FilterPresetDisplay
{
    /// <summary>A row of pressed-state buttons.</summary>
    Buttons,
    /// <summary>A compact dropdown.</summary>
    Dropdown
}

/// <summary>Current value and change callback for a custom field editor.</summary>
public sealed class FilterValueEditorContext
{
    private readonly FilterCondition condition;
    private readonly EventCallback changed;

    internal FilterValueEditorContext(FilterField field, FilterCondition condition, EventCallback changed)
    {
        Field = field;
        this.condition = condition;
        this.changed = changed;
    }

    /// <summary>The field being edited.</summary>
    public FilterField Field { get; }
    /// <summary>The selected comparison operator.</summary>
    public FilterOperator Operator => condition.Operator;
    /// <summary>The current value.</summary>
    public object? Value => condition.Value;
    /// <summary>The upper value for range comparisons.</summary>
    public object? ValueEnd => condition.ValueEnd;

    /// <summary>Updates the active draft and uses the builder's normal apply/debounce behavior.</summary>
    public async Task SetValueAsync(object? value, object? valueEnd = null)
    {
        condition.Value = value;
        condition.ValueEnd = valueEnd;
        await changed.InvokeAsync();
    }
}
