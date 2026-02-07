using BlazorBlueprint.Components.Command;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Combobox;

/// <summary>
/// A searchable combobox component that enables users to filter and select from a list of options.
/// </summary>
/// <typeparam name="TItem">The type of items in the combobox list.</typeparam>
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
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Combobox TItem="Framework"
///           Items="frameworks"
///           @bind-Value="selectedFramework"
///           ValueSelector="@(f => f.Value)"
///           DisplaySelector="@(f => f.Label)"
///           Placeholder="Select framework..."
///           SearchPlaceholder="Search framework..."
///           EmptyMessage="No framework found." /&gt;
/// </code>
/// </example>
public partial class Combobox<TItem> : ComponentBase
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
    /// Gets or sets the callback that is invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the function to extract the value from an item.
    /// </summary>
    /// <remarks>
    /// SECURITY: This function should return a plain string value only.
    /// Never return MarkupString or unencoded HTML to prevent XSS vulnerabilities.
    /// </remarks>
    [Parameter, EditorRequired]
    public Func<TItem, string> ValueSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the function to extract the display text from an item.
    /// </summary>
    /// <remarks>
    /// SECURITY: This function should return a plain string value only.
    /// Never return MarkupString or unencoded HTML to prevent XSS vulnerabilities.
    /// Blazor will automatically HTML-encode the output when rendered with @ syntax.
    /// </remarks>
    [Parameter, EditorRequired]
    public Func<TItem, string> DisplaySelector { get; set; } = default!;

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
    /// Tracks whether the popover is currently open.
    /// </summary>
    private bool _isOpen { get; set; }

    /// <summary>
    /// Reference to the CommandInput for focus management.
    /// </summary>
    private CommandInput? _commandInputRef;

    /// <summary>
    /// Tracks whether focus has been done for the current open.
    /// </summary>
    private bool _focusDone;

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

        if (ValueSelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(Combobox<TItem>)}: {nameof(ValueSelector)} parameter is required.");
        }

        if (DisplaySelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(Combobox<TItem>)}: {nameof(DisplaySelector)} parameter is required.");
        }

        // Filter out null items for safety
        Items = Items?.Where(item => item != null) ?? Enumerable.Empty<TItem>();
    }

    /// <summary>
    /// Gets the display text for the currently selected item.
    /// </summary>
    private string SelectedDisplayText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                return Placeholder;
            }

            var selectedItem = Items.FirstOrDefault(item => ValueSelector(item) == Value);
            return selectedItem != null ? DisplaySelector(selectedItem) : Placeholder;
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
    /// Resets focus tracking when the popover closes.
    /// </summary>
    /// <param name="isOpen">Whether the popover is now open.</param>
    private void HandleOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        if (!isOpen)
        {
            _focusDone = false; // Reset for next open
        }
    }

    /// <summary>
    /// Handles item selection with toggle behavior.
    /// </summary>
    /// <param name="item">The item that was selected.</param>
    private async Task HandleSelect(TItem item)
    {
        var itemValue = ValueSelector(item);

        // Toggle behavior: if already selected, deselect
        var newValue = Value == itemValue ? null : itemValue;

        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);

        // Close the popover after selection
        _isOpen = false;
        // Note: _focusDone is reset by HandleOpenChanged
    }

    /// <summary>
    /// Checks if an item is currently selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected; otherwise, false.</returns>
    private bool IsSelected(TItem item) => Value == ValueSelector(item);

    /// <summary>
    /// Gets the CSS class for the combobox container.
    /// </summary>
    private static string ContainerClass => "relative";

    /// <summary>
    /// Gets the CSS class for the button element (styled like ButtonVariant.Outline).
    /// </summary>
    private string ButtonCssClass => ClassNames.cn(
        "inline-flex items-center justify-between rounded-md text-sm font-medium",
        "transition-colors focus-visible:outline-none focus-visible:ring-2",
        "focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:opacity-50 disabled:pointer-events-none",
        "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        "h-9 px-3",
        string.IsNullOrWhiteSpace(Class) ? PopoverWidth : null,
        Class
    );
}
