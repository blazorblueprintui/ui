using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbCombobox{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public partial class BbFormFieldCombobox<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the collection of options to display in the combobox.
    /// When provided, uses Options mode. When null, uses Compositional mode (ChildContent).
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<TValue>>? Options { get; set; }

    /// <summary>
    /// Gets or sets the child content for compositional mode.
    /// Use ComboboxItem components as children for rich item rendering.
    /// Only used when Options is null.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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
    /// Gets or sets the current search query text.
    /// Use with <see cref="SearchQueryChanged"/> for two-way binding to react to filter changes,
    /// e.g. for server-side filtering or loading additional data.
    /// </summary>
    [Parameter]
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback that is invoked when the search query changes.
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
    /// When true, a loading spinner is shown at the bottom of the dropdown list and
    /// <see cref="OnLoadMore"/> is suppressed until loading completes.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets a message displayed at the bottom of the dropdown list when all items have been loaded.
    /// Only shown when <see cref="IsLoading"/> is false. Set to <c>null</c> or empty to hide.
    /// </summary>
    [Parameter]
    public string? EndOfListMessage { get; set; }

    /// <summary>
    /// Gets or sets whether the combobox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the combobox is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

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

    /// <summary>
    /// Gets or sets CSS classes applied to the trigger when the dropdown is open.
    /// Default is <c>"bg-accent text-accent-foreground"</c>.
    /// Set to <c>null</c> or empty to disable the active style.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; } = "bg-accent text-accent-foreground";

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }

    private async Task HandleSearchQueryChanged(string query)
    {
        SearchQuery = query;
        await SearchQueryChanged.InvokeAsync(query);
    }
}
