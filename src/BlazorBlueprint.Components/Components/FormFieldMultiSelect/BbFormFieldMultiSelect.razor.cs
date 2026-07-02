using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbMultiSelect{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TValue">The type of the selected values.</typeparam>
public partial class BbFormFieldMultiSelect<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the collection of options to display in the multiselect.
    /// When provided, uses Options mode. When null, uses Compositional mode (ChildContent).
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<TValue>>? Options { get; set; }

    /// <summary>
    /// Gets or sets the child content for compositional mode.
    /// Use MultiSelectItem components as children for rich item rendering.
    /// Only used when Options is null.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IEnumerable<TValue>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TValue>?> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound values for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<IEnumerable<TValue>?>>? ValuesExpression { get; set; }

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

    /// <summary>
    /// Gets or sets the callback invoked when the search query changes.
    /// Use for server-side filtering or loading external data.
    /// </summary>
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

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
    protected override LambdaExpression? GetFieldExpression() => ValuesExpression;

    private async Task HandleValuesChanged(IEnumerable<TValue>? values)
    {
        Values = values;
        await ValuesChanged.InvokeAsync(values);
        NotifyFieldChanged();
    }
}
