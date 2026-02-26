using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A searchable combobox component that enables users to filter and select from a list of options.
/// Supports two modes: Options mode (data-driven) and Compositional mode (ChildContent with ComboboxItem children).
/// </summary>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
/// <remarks>
/// <para>
/// The Combobox component combines autocomplete functionality with a dropdown interface.
/// It internally composes Popover, Command, and Button components to provide a searchable
/// selection experience.
/// </para>
/// <para>
/// Features:
/// - Generic type support for flexible data binding
/// - Two-way binding with @bind-Value
/// - Search/filter functionality (case-insensitive)
/// - Single selection with toggle behavior
/// - Check icon for selected item
/// - Empty state when no matches found
/// - Keyboard navigation support
/// - Accessibility: ARIA attributes, keyboard support
/// - Dual mode: Options for simple lists, ChildContent with ComboboxItem for rich rendering
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Options mode --&gt;
/// &lt;Combobox TValue="string"
///           Options="frameworkOptions"
///           @bind-Value="selectedFramework"
///           Placeholder="Select framework..."
///           SearchPlaceholder="Search framework..."
///           EmptyMessage="No framework found." /&gt;
///
/// &lt;!-- Compositional mode --&gt;
/// &lt;Combobox TValue="string" @bind-Value="selectedFramework"
///           Placeholder="Select framework..." SearchPlaceholder="Search..."&gt;
///     &lt;ComboboxItem TValue="string" Value="blazor" Text="Blazor"&gt;Blazor&lt;/ComboboxItem&gt;
/// &lt;/Combobox&gt;
/// </code>
/// </example>
public partial class BbCombobox<TValue> : ComponentBase
{
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;
    private static readonly Func<CommandItemMetadata, string, bool> PassthroughFilter = (_, _) => true;
    private Func<CommandItemMetadata, string, bool>? _filterFunction;
    private string? _selectedDisplayTextCache;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the collection of options to display in the combobox.
    /// When provided, uses Options mode (data-driven). When null, uses Compositional mode (ChildContent).
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
    /// Gets or sets the callback that is invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown in the button when no item is selected.
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
    /// <remarks>
    /// When the popover closes, the search query is reset to <see cref="string.Empty"/> and
    /// <see cref="SearchQueryChanged"/> is invoked so the consumer can reload its default dataset
    /// (e.g. initial top-N results) for the next open.
    /// </remarks>
    [Parameter]
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback that is invoked when the search query changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the combobox container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the combobox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the ID(s) of the element(s) that describe this combobox for accessibility.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets the width of the popover content.
    /// </summary>
    /// <remarks>
    /// Defaults to "w-[200px]". Can be overridden with Tailwind classes.
    /// Ignored when MatchTriggerWidth is true.
    /// </remarks>
    [Parameter]
    public string PopoverWidth { get; set; } = "w-[200px]";

    /// <summary>
    /// Gets or sets whether to match the dropdown width to the trigger element width.
    /// When true, PopoverWidth is ignored.
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

    /// <summary>
    /// Tracks whether the popover is currently open.
    /// </summary>
    private bool _isOpen { get; set; }

    /// <summary>
    /// Reference to the CommandInput for focus management.
    /// </summary>
    private BbCommandInput? _commandInputRef;

    /// <summary>
    /// Tracks whether focus has been done for the current open.
    /// </summary>
    private bool _focusDone;

    /// <summary>
    /// Item text registry for compositional mode display text lookup.
    /// </summary>
#pragma warning disable CS8714 // TValue may be null but dictionary keys are non-null in practice
    private readonly Dictionary<TValue, string> _itemTextRegistry = new(EqualityComparer<TValue>.Default);
#pragma warning restore CS8714

    /// <summary>
    /// Gets a unique identifier for this combobox instance.
    /// </summary>
    private string Id { get; set; } = $"combobox-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets whether the popover is currently open.
    /// </summary>
    private bool GetIsOpen() => _isOpen;

    /// <summary>
    /// Validates required parameters.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Filter out null options for safety (Options mode only)
        Options = Options?.Where(o => o != null);

        // When the consumer handles filtering externally via SearchQueryChanged,
        // bypass the Command's internal text filter to avoid double-filtering and race conditions.
        _filterFunction = SearchQueryChanged.HasDelegate ? PassthroughFilter : null;

        if (CascadedEditContext != null && ValueExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    /// <summary>
    /// Gets the display text for the currently selected item.
    /// Checks Options first (Options mode), then the item text registry (Compositional mode),
    /// then falls back to the cached display text from the last selection.
    /// </summary>
    private string SelectedDisplayText
    {
        get
        {
            if (Value is null)
            {
                return Placeholder;
            }

            // Options mode: look up from Options collection
            var selectedOption = Options?.FirstOrDefault(o => EqualityComparer<TValue>.Default.Equals(o.Value, Value));
            if (selectedOption is not null)
            {
                return selectedOption.Text;
            }

            // Compositional mode: look up from registered items
            if (_itemTextRegistry.GetValueOrDefault(Value) is { } registryText)
            {
                return registryText;
            }

            // Fallback: cached display text from last selection survives Options array changes
            // during async filtering (e.g. selected option filtered out of current results).
            return _selectedDisplayTextCache ?? Placeholder;
        }
    }

    /// <summary>
    /// Handles the popover content ready event to focus the search input.
    /// This is called when the popover is fully positioned and visible.
    /// </summary>
    private async Task HandleContentReady()
    {
        // Guard against multiple calls per open
        if (_focusDone)
        {
            return;
        }

        _focusDone = true;

        if (_commandInputRef == null)
        {
            return;
        }

        try
        {
            // Small delay to let browser finish processing DOM changes
            await Task.Delay(50);
            await _commandInputRef.FocusAsync();
        }
        catch
        {
            // Ignore focus errors
        }
    }

    /// <summary>
    /// Handles the open state change of the popover.
    /// Resets focus tracking and search query when the popover closes.
    /// </summary>
    /// <param name="isOpen">Whether the popover is now open.</param>
    private async Task HandleOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        if (!isOpen)
        {
            _focusDone = false; // Reset for next open

            // Reset search query and notify the consumer so it can reload
            // the default dataset (e.g. initial top-N results) for the next open.
            SearchQuery = string.Empty;
            await SearchQueryChanged.InvokeAsync(string.Empty);
        }
    }

    /// <summary>
    /// Handles the search query change from the inner Command component.
    /// </summary>
    private async Task HandleSearchQueryChanged(string query)
    {
        SearchQuery = query;
        await SearchQueryChanged.InvokeAsync(query);
    }

    /// <summary>
    /// Handles item selection with toggle behavior.
    /// </summary>
    /// <param name="option">The option that was selected.</param>
    private async Task HandleSelect(SelectOption<TValue> option)
    {
        // Toggle behavior: if already selected, deselect
        var newValue = EqualityComparer<TValue>.Default.Equals(Value, option.Value) ? default : option.Value;

        // Cache display text so it survives async Options changes where
        // the selected option may no longer be in the current results.
        _selectedDisplayTextCache = newValue is not null ? option.Text : null;

        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);

        if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }

        // Close the popover after selection
        _isOpen = false;
        // Note: _focusDone is reset by HandleOpenChanged
    }

    /// <summary>
    /// Checks if an option is currently selected.
    /// </summary>
    /// <param name="option">The option to check.</param>
    /// <returns>True if the option is selected; otherwise, false.</returns>
    private bool IsSelected(SelectOption<TValue> option) => EqualityComparer<TValue>.Default.Equals(Value, option.Value);

    /// <summary>
    /// Gets the CSS class for the combobox container.
    /// </summary>
    private static string ContainerClass => "relative";

    /// <summary>
    /// Gets the CSS class for the button element (styled like ButtonVariant.Outline).
    /// </summary>
    private string ButtonCssClass => ClassNames.cn(
        "inline-flex items-center justify-between rounded-md text-sm font-medium",
        "transition-colors focus-visible:outline-none",
        "disabled:opacity-50 disabled:pointer-events-none",
        "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        _isOpen ? ActiveClass : null,
        "h-9 px-3",
        string.IsNullOrWhiteSpace(Class) ? PopoverWidth : null,
        Class
    );

    // ── Compositional mode support (used by ComboboxItem) ──────────────

    /// <summary>
    /// Registers an item's value and display text for trigger display text lookup.
    /// Called by ComboboxItem on initialization.
    /// </summary>
    internal void RegisterItem(TValue value, string text)
    {
        if (value is not null)
        {
            _itemTextRegistry[value] = text;
        }
    }

    /// <summary>
    /// Unregisters an item's value from the text registry.
    /// Called by ComboboxItem on disposal.
    /// </summary>
    internal void UnregisterItem(TValue value)
    {
        if (value is not null)
        {
            _itemTextRegistry.Remove(value);
        }
    }

    /// <summary>
    /// Checks if a value is currently selected.
    /// Used by ComboboxItem to render the check icon.
    /// </summary>
    internal bool IsValueSelected(TValue? value) =>
        EqualityComparer<TValue>.Default.Equals(Value, value);

    /// <summary>
    /// Handles item selection from a ComboboxItem child component.
    /// Constructs a SelectOption and delegates to HandleSelect.
    /// </summary>
    internal async Task HandleItemSelected(TValue value, string text) =>
        await HandleSelect(new SelectOption<TValue>(value, text));
}
