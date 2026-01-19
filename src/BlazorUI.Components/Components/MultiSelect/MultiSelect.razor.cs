using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using System.Text;

namespace BlazorUI.Components.MultiSelect;

/// <summary>
/// A multi-select component that allows users to select multiple options from a searchable dropdown.
/// </summary>
/// <typeparam name="TItem">The type of items in the multiselect list.</typeparam>
/// <remarks>
/// <para>
/// The MultiSelect component combines search/filter functionality with a multi-selection dropdown.
/// Selected items are displayed as dismissible tags. It includes keyboard navigation and
/// accessibility features.
/// </para>
/// <para>
/// Features:
/// - Generic type support for flexible data binding
/// - Two-way binding with @bind-Values
/// - Search/filter functionality (case-insensitive)
/// - Multiple selection with checkbox indicators
/// - Select All option with indeterminate state
/// - Selected items displayed as tags
/// - "+N more" overflow indicator
/// - Form validation integration (EditContext)
/// - Keyboard navigation support
/// - Accessibility: ARIA attributes, keyboard support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;MultiSelect TItem="Language"
///              Items="languages"
///              @bind-Values="selectedLanguages"
///              ValueSelector="@(l => l.Code)"
///              DisplaySelector="@(l => l.Name)"
///              Placeholder="Select languages..."
///              SearchPlaceholder="Search..."
///              ShowSelectAll="true" /&gt;
/// </code>
/// </example>
public partial class MultiSelect<TItem> : ComponentBase
{
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

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
    /// Gets or sets the callback that is invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<string>?> ValuesChanged { get; set; }

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
    /// Gets or sets additional CSS classes to apply to the multiselect container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

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
    /// Gets or sets the width of the popover content.
    /// </summary>
    [Parameter]
    public string PopoverWidth { get; set; } = "w-[300px]";

    /// <summary>
    /// Gets or sets an expression that identifies the bound values.
    /// Used for form validation integration.
    /// </summary>
    [Parameter]
    public Expression<Func<IEnumerable<string>?>>? ValuesExpression { get; set; }

    /// <summary>
    /// Tracks whether the popover is currently open.
    /// </summary>
    private bool _isOpen { get; set; } = false;

    /// <summary>
    /// Tracks the current search query for filtering.
    /// </summary>
    private string _searchQuery = string.Empty;

    /// <summary>
    /// Gets a unique identifier for this multiselect instance.
    /// </summary>
    private string Id { get; set; } = $"multiselect-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the list of currently selected values.
    /// </summary>
    private List<string> SelectedValues => Values?.ToList() ?? new List<string>();

    /// <summary>
    /// Gets the number of selected items that exceed MaxDisplayTags.
    /// </summary>
    private int OverflowCount => Math.Max(0, SelectedValues.Count - MaxDisplayTags);

    /// <summary>
    /// Gets whether any items are selected.
    /// </summary>
    private bool HasSelectedItems => SelectedValues.Count > 0;

    /// <summary>
    /// Gets whether there are any items visible after filtering.
    /// </summary>
    private bool HasFilteredItems => GetFilteredItems().Any();

    /// <summary>
    /// Gets the items that match the current search query.
    /// </summary>
    private IEnumerable<TItem> GetFilteredItems()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return Items;
        }

        return Items.Where(item =>
            DisplaySelector(item).Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets whether the multiselect is in an invalid state (for validation).
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            if (_editContext != null && ValuesExpression != null && _fieldIdentifier.FieldName != null)
            {
                return _editContext.GetValidationMessages(_fieldIdentifier).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Validates required parameters.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ValueSelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(MultiSelect<TItem>)}: {nameof(ValueSelector)} parameter is required.");
        }

        if (DisplaySelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(MultiSelect<TItem>)}: {nameof(DisplaySelector)} parameter is required.");
        }

        // Filter out null items for safety
        Items = Items?.Where(item => item != null) ?? Enumerable.Empty<TItem>();

        // Initialize EditContext integration if available
        if (CascadedEditContext != null && ValuesExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValuesExpression);
        }
    }

    /// <summary>
    /// Opens the dropdown.
    /// </summary>
    private void Open()
    {
        if (Disabled) return;
        _isOpen = true;
    }

    /// <summary>
    /// Closes the dropdown.
    /// </summary>
    private void Close()
    {
        _isOpen = false;
        _searchQuery = string.Empty;
    }

    /// <summary>
    /// Handles search query changes from the Command component.
    /// </summary>
    private void HandleSearchQueryChanged(string query)
    {
        _searchQuery = query;
    }

    /// <summary>
    /// Toggles the dropdown.
    /// </summary>
    private void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// Handles item toggle (selection/deselection).
    /// </summary>
    /// <param name="item">The item that was toggled.</param>
    private async Task HandleToggle(TItem item)
    {
        var itemValue = ValueSelector(item);
        var currentValues = SelectedValues;

        if (currentValues.Contains(itemValue))
        {
            currentValues.Remove(itemValue);
        }
        else
        {
            currentValues.Add(itemValue);
        }

        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Handles Select All toggle.
    /// </summary>
    private async Task HandleSelectAllToggle()
    {
        var allValues = Items.Select(ValueSelector).ToList();
        var currentValues = SelectedValues;

        if (currentValues.Count == allValues.Count)
        {
            // All selected, deselect all
            await UpdateValues(new List<string>());
        }
        else
        {
            // Select all
            await UpdateValues(allValues);
        }
    }

    /// <summary>
    /// Removes a value from the selection.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    private async Task RemoveValue(string value)
    {
        var currentValues = SelectedValues;
        currentValues.Remove(value);
        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    private async Task ClearAll()
    {
        await UpdateValues(new List<string>());
    }

    /// <summary>
    /// Updates the selected values and notifies parent.
    /// </summary>
    /// <param name="values">The new values.</param>
    private async Task UpdateValues(List<string> values)
    {
        Values = values.Count > 0 ? values : null;
        await ValuesChanged.InvokeAsync(Values);

        // Notify EditContext of field change for validation
        if (_editContext != null && ValuesExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }

        // Force re-render to update checkbox states
        StateHasChanged();
    }

    /// <summary>
    /// Checks if an item is currently selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected; otherwise, false.</returns>
    private bool IsSelected(TItem item)
    {
        return SelectedValues.Contains(ValueSelector(item));
    }

    /// <summary>
    /// Gets the Select All state (None, Indeterminate, All).
    /// </summary>
    private SelectAllState GetSelectAllState()
    {
        var totalCount = Items.Count();
        var selectedCount = SelectedValues.Count;

        if (selectedCount == 0) return SelectAllState.None;
        if (selectedCount == totalCount) return SelectAllState.All;
        return SelectAllState.Indeterminate;
    }

    /// <summary>
    /// Gets the display text for a value.
    /// </summary>
    /// <param name="value">The value to get display text for.</param>
    /// <returns>The display text.</returns>
    private string GetDisplayText(string value)
    {
        var item = Items.FirstOrDefault(i => ValueSelector(i) == value);
        return item != null ? DisplaySelector(item) : value;
    }

    /// <summary>
    /// Gets the CSS class for the multiselect container.
    /// </summary>
    private string ContainerClass => "relative";

    /// <summary>
    /// Gets the CSS class for the trigger button.
    /// </summary>
    private string TriggerCssClass
    {
        get
        {
            var builder = new StringBuilder();

            // Base button styles
            builder.Append("inline-flex items-center justify-between rounded-md text-sm font-medium ");
            builder.Append("transition-colors focus-visible:outline-none focus-visible:ring-2 ");
            builder.Append("focus-visible:ring-ring focus-visible:ring-offset-2 ");
            builder.Append("disabled:opacity-50 disabled:pointer-events-none ");

            // Outline variant styles
            builder.Append("border border-input bg-background hover:bg-accent hover:text-accent-foreground ");

            // Size styles
            builder.Append("min-h-9 px-3 py-1.5 ");

            // Width
            builder.Append(PopoverWidth);
            builder.Append(' ');

            // Custom classes
            if (!string.IsNullOrWhiteSpace(Class))
            {
                builder.Append(Class);
            }

            return builder.ToString().Trim();
        }
    }

    /// <summary>
    /// Gets the CSS class for the tag.
    /// </summary>
    private string TagCssClass =>
        "inline-flex items-center gap-1 rounded-full border bg-secondary px-2 py-0.5 text-xs font-semibold transition-colors";

    /// <summary>
    /// Gets the CSS class for the tag remove button.
    /// </summary>
    private string TagRemoveButtonCssClass =>
        "ml-0.5 rounded-full outline-none hover:bg-secondary-foreground/20 focus:ring-1 focus:ring-ring";

    /// <summary>
    /// Gets the CSS class for the dropdown item.
    /// </summary>
    private string ItemCssClass =>
        "relative flex cursor-pointer select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none " +
        "data-[focused=true]:bg-accent data-[focused=true]:text-accent-foreground " +
        "data-[disabled=true]:pointer-events-none data-[disabled=true]:opacity-50";

    /// <summary>
    /// Gets the CSS class for the checkbox.
    /// </summary>
    private string CheckboxCssClass =>
        "h-4 w-4 shrink-0 rounded-sm border border-primary ring-offset-background " +
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 " +
        "disabled:cursor-not-allowed disabled:opacity-50";

    /// <summary>
    /// Gets the CSS class for a checked checkbox.
    /// </summary>
    private string CheckboxCheckedCssClass =>
        CheckboxCssClass + " bg-primary text-primary-foreground";

    /// <summary>
    /// Gets the CSS class for an unchecked checkbox.
    /// </summary>
    private string CheckboxUncheckedCssClass =>
        CheckboxCssClass + " bg-background";
}

/// <summary>
/// Represents the state of the Select All checkbox.
/// </summary>
public enum SelectAllState
{
    /// <summary>No items are selected.</summary>
    None,
    /// <summary>Some items are selected.</summary>
    Indeterminate,
    /// <summary>All items are selected.</summary>
    All
}
