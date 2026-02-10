using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldMultiSelect;

/// <summary>
/// A form field wrapper for <see cref="MultiSelect.MultiSelect{TItem}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TItem">The type of items in the multiselect list.</typeparam>
public partial class FormFieldMultiSelect<TItem> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the collection of items to display in the multiselect.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<string>?> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound values for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<IEnumerable<string>?>>? ValuesExpression { get; set; }

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
    /// Gets or sets the placeholder text shown when no items are selected.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select items...";

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
    /// Gets or sets the label for the Select All option.
    /// </summary>
    [Parameter]
    public string SelectAllLabel { get; set; } = "Select All";

    /// <summary>
    /// Gets or sets whether to show the Select All option.
    /// </summary>
    [Parameter]
    public bool ShowSelectAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the label for the Clear button.
    /// </summary>
    [Parameter]
    public string ClearLabel { get; set; } = "Clear";

    /// <summary>
    /// Gets or sets the label for the Close button.
    /// </summary>
    [Parameter]
    public string CloseLabel { get; set; } = "Close";

    /// <summary>
    /// Gets or sets whether the multiselect is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tags to display before showing "+N more".
    /// </summary>
    [Parameter]
    public int MaxDisplayTags { get; set; } = 3;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner MultiSelect element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the width of the popover content.
    /// </summary>
    [Parameter]
    public string PopoverWidth { get; set; } = "w-[300px]";

    /// <summary>
    /// Gets or sets whether to match the dropdown width to the trigger element width.
    /// </summary>
    [Parameter]
    public bool MatchTriggerWidth { get; set; }

    /// <summary>
    /// Gets or sets whether clicking outside the dropdown should close it.
    /// </summary>
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValuesExpression;

    private async Task HandleValuesChanged(IEnumerable<string>? values)
    {
        Values = values;
        await ValuesChanged.InvokeAsync(values);
    }
}
