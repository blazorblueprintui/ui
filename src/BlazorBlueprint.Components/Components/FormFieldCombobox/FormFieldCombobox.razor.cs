using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldCombobox;

/// <summary>
/// A form field wrapper for <see cref="Combobox.Combobox{TItem}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TItem">The type of items in the combobox list.</typeparam>
public partial class FormFieldCombobox<TItem> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the collection of items to display in the combobox.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the function to extract the value from an item.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> ValueSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the function to extract the display text from an item.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> DisplaySelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the placeholder text shown when no item is selected.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select an option...";

    /// <summary>
    /// Gets or sets the placeholder text shown in the search input.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// Gets or sets the message displayed when no items match the search.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No results found.";

    /// <summary>
    /// Gets or sets whether the combobox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Combobox element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the width of the popover content.
    /// </summary>
    [Parameter]
    public string PopoverWidth { get; set; } = "w-[200px]";

    /// <summary>
    /// Gets or sets whether to match the dropdown width to the trigger element width.
    /// </summary>
    [Parameter]
    public bool MatchTriggerWidth { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(string? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
