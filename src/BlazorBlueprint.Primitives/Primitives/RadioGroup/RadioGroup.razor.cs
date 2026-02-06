using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Primitives.RadioGroup;

/// <summary>
/// A headless radio group primitive that provides behavior and accessibility without styling.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with radio group items.</typeparam>
/// <remarks>
/// <para>
/// The RadioGroup component is a headless primitive that handles all radio group behavior,
/// keyboard navigation (arrow keys), and accessibility features. It provides no default styling.
/// </para>
/// <para>
/// This primitive manages a group of mutually exclusive radio options, where only one option
/// can be selected at a time. It provides context to child RadioGroupItem components
/// via Blazor's CascadingValue mechanism.
/// </para>
/// <para>
/// Accessibility features (WCAG 2.1 AA):
/// <list type="bullet">
/// <item>Semantic radiogroup role</item>
/// <item>Arrow key navigation (Up/Down, Left/Right)</item>
/// <item>aria-disabled attribute for disabled state</item>
/// <item>aria-label for screen reader context</item>
/// <item>Automatic focus management during navigation</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic headless radio group:
/// <code>
/// &lt;RadioGroup @bind-Value="selectedOption"&gt;
///     &lt;RadioGroupItem Value="@("option1")" /&gt;
///     &lt;RadioGroupItem Value="@("option2")" /&gt;
/// &lt;/RadioGroup&gt;
/// </code>
/// </example>
public partial class RadioGroup<TValue> : ComponentBase
{
    private RadioGroupContext<TValue> context = new();
    private List<RadioGroupItem<TValue>>? _cachedEnabledItems;
    private int _lastItemsVersion;

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    /// <remarks>
    /// This property supports two-way binding using the @bind-Value directive.
    /// Changes to this property trigger the ValueChanged event callback.
    /// </remarks>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    /// <remarks>
    /// This event callback enables two-way binding with @bind-Value.
    /// It is invoked whenever a radio button is selected.
    /// </remarks>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the entire radio group is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled, all radio items in the group cannot be selected.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the radio group.
    /// </summary>
    /// <remarks>
    /// Provides accessible text for screen readers to describe the purpose of the radio group.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the radio group.
    /// </summary>
    /// <remarks>
    /// Should contain RadioGroupItem components.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Selects a value and notifies listeners.
    /// </summary>
    /// <param name="value">The value to select.</param>
    private async Task SelectValueAsync(TValue value)
    {
        if (Disabled)
        {
            return;
        }

        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    /// <summary>
    /// Checks if the given value is the currently selected value.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is selected, false otherwise.</returns>
    private bool IsSelected(TValue? value)
    {
        if (Value == null && value == null)
        {
            return true;
        }

        if (Value == null || value == null)
        {
            return false;
        }

        return EqualityComparer<TValue>.Default.Equals(Value, value);
    }

    /// <summary>
    /// Handles keyboard navigation for the radio group.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <remarks>
    /// Implements WCAG 2.1 keyboard accessibility:
    /// - Arrow Down/Right: Navigate to next radio item
    /// - Arrow Up/Left: Navigate to previous radio item
    /// - Automatically selects the navigated item and focuses it
    /// - Prevents default browser scroll behavior for arrow keys
    /// </remarks>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (Disabled)
        {
            return;
        }

        var enabledItems = GetEnabledItems();
        if (enabledItems.Count == 0)
        {
            return;
        }


        switch (args.Key)
        {
            case "ArrowDown":
            case "ArrowRight":
                await NavigateNext(enabledItems);
                break;

            case "ArrowUp":
            case "ArrowLeft":
                await NavigatePrevious(enabledItems);
                break;
        }
    }

    /// <summary>
    /// Gets the list of enabled items, using a cached version when possible
    /// to avoid allocations on every keyboard navigation.
    /// </summary>
    private List<RadioGroupItem<TValue>> GetEnabledItems()
    {
        // Use items count as a simple version check - if count changed, items changed
        var currentVersion = context.Items.Count;
        if (_cachedEnabledItems == null || _lastItemsVersion != currentVersion)
        {
            _cachedEnabledItems = context.Items.Where(i => !i.Disabled).ToList();
            _lastItemsVersion = currentVersion;
        }
        return _cachedEnabledItems;
    }

    /// <summary>
    /// Navigates to the next radio item.
    /// </summary>
    private async Task NavigateNext(List<RadioGroupItem<TValue>> enabledItems)
    {
        if (enabledItems.Count == 0)
        {
            return;
        }

        var currentIndex = enabledItems.FindIndex(i => IsSelected(i.Value));
        var nextIndex = (currentIndex + 1) % enabledItems.Count;

        var nextItem = enabledItems[nextIndex];
        if (nextItem.Value != null)
        {
            await SelectValueAsync(nextItem.Value);
            await nextItem.FocusAsync();
        }
    }

    /// <summary>
    /// Navigates to the previous radio item.
    /// </summary>
    private async Task NavigatePrevious(List<RadioGroupItem<TValue>> enabledItems)
    {
        if (enabledItems.Count == 0)
        {
            return;
        }

        var currentIndex = enabledItems.FindIndex(i => IsSelected(i.Value));
        var prevIndex = currentIndex <= 0 ? enabledItems.Count - 1 : currentIndex - 1;

        var prevItem = enabledItems[prevIndex];
        if (prevItem.Value != null)
        {
            await SelectValueAsync(prevItem.Value);
            await prevItem.FocusAsync();
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Update context with current state
        context.Value = Value;
        context.Disabled = Disabled;
        context.SelectValue = SelectValueAsync;
    }
}
