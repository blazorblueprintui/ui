using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for the Select component that provides
/// automatic label, helper text, and error message display with a simplified API.
/// </summary>
/// <remarks>
/// FormFieldSelect auto-generates the SelectTrigger, SelectValue, and SelectContent.
/// Users only need to provide SelectItem children as ChildContent.
/// </remarks>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public partial class BbFormFieldSelect<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the select is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when no value is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets a function to derive display text from the selected value.
    /// </summary>
    [Parameter]
    public Func<TValue, string>? DisplayTextSelector { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Select element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the collection of value/label pairs to display.
    /// Each option contains a value (bound on selection) and display text (shown in the dropdown and trigger).
    /// When provided, SelectItem children are auto-generated. Takes priority over <see cref="ChildContent"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<TValue>>? Options { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the select dropdown.
    /// Should contain SelectItem components. Ignored when Options is provided.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user scrolls near the bottom of the dropdown list.
    /// Use this to load additional items for infinite scroll scenarios.
    /// </summary>
    [Parameter]
    public EventCallback OnLoadMore { get; set; }

    /// <summary>
    /// Gets or sets whether additional items are currently being loaded.
    /// When true, a loading spinner is shown at the bottom of the list and
    /// <see cref="OnLoadMore"/> is suppressed until loading completes.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets a message displayed at the bottom of the list when all items have been loaded.
    /// Only shown when <see cref="IsLoading"/> is false. Set to <c>null</c> or empty to hide.
    /// </summary>
    [Parameter]
    public string? EndOfListMessage { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }

    private Func<TValue, string>? GetEffectiveDisplayTextSelector()
    {
        if (Options is not null)
        {
            var options = Options;
            return v => options
                .FirstOrDefault(o => EqualityComparer<TValue>.Default.Equals(o.Value, v))
                ?.Text ?? v?.ToString() ?? "";
        }

        return DisplayTextSelector;
    }
}
