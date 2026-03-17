using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbCheckboxGroup{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TValue">The type of each checkbox value.</typeparam>
public partial class BbFormFieldCheckboxGroup<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TValue> Values { get; set; } = Array.Empty<TValue>();

    /// <summary>
    /// Gets or sets the callback invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TValue>> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets whether to show a "Select all" checkbox.
    /// </summary>
    [Parameter]
    public bool ShowSelectAll { get; set; }

    /// <summary>
    /// Gets or sets the label for the select-all checkbox.
    /// </summary>
    [Parameter]
    public string SelectAllLabel { get; set; } = "Select all";

    /// <summary>
    /// Gets or sets whether all items in the group are disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the CheckboxGroupItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner CheckboxGroup.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => null;

    private async Task HandleValuesChanged(IReadOnlyCollection<TValue> values)
    {
        Values = values;
        await ValuesChanged.InvokeAsync(values);
    }
}
