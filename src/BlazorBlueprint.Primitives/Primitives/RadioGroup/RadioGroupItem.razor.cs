using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Primitives.RadioGroup;

/// <summary>
/// A headless radio button item primitive that can be selected within a RadioGroup.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with this radio item.</typeparam>
/// <remarks>
/// <para>
/// The RadioGroupItem component represents a single selectable option within a RadioGroup.
/// It provides no default styling, only behavior and accessibility features.
/// </para>
/// <para>
/// This component must be used as a child of a RadioGroup component.
/// It receives the radio group context via Blazor's CascadingParameter.
/// </para>
/// <para>
/// Accessibility features (WCAG 2.1 AA):
/// <list type="bullet">
/// <item>Semantic button element with radio role</item>
/// <item>aria-checked attribute reflects selected state</item>
/// <item>aria-disabled attribute for disabled state</item>
/// <item>aria-label for screen reader context</item>
/// <item>Keyboard support (Space/Enter to select)</item>
/// <item>Proper tabindex for roving tab index pattern</item>
/// <item>data-state attribute for CSS styling hooks</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic headless radio item:
/// <code>
/// &lt;RadioGroupItem Value="@("option1")" /&gt;
/// </code>
///
/// Styled radio item with custom classes:
/// <code>
/// &lt;RadioGroupItem Value="@("option1")"
///                          class="h-4 w-4 rounded-full border"&gt;
///     &lt;span class="inner-circle"&gt;&lt;/span&gt;
/// &lt;/RadioGroupItem&gt;
/// </code>
/// </example>
public partial class RadioGroupItem<TValue> : ComponentBase, IDisposable
{
    private ElementReference buttonElement;
    private bool shouldPreventDefault;

    /// <summary>
    /// Gets or sets the parent RadioGroup context.
    /// </summary>
    [CascadingParameter]
    private RadioGroupContext<TValue>? Context { get; set; }

    /// <summary>
    /// Gets or sets the value associated with this radio item.
    /// </summary>
    /// <remarks>
    /// When this item is selected, this value becomes the RadioGroup's Value.
    /// </remarks>
    [Parameter, EditorRequired]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets whether this individual radio item is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled, the item cannot be selected and appears with reduced opacity.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the ID attribute for the radio item element.
    /// </summary>
    /// <remarks>
    /// Used for associating the radio item with label elements via htmlFor attribute.
    /// Auto-generated if not provided.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the radio item.
    /// </summary>
    /// <remarks>
    /// Provides accessible text for screen readers when the radio item
    /// doesn't have an associated label element.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the radio item.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets whether this radio item is checked.
    /// </summary>
    private bool IsChecked
    {
        get
        {
            if (Context == null)
            {
                return false;
            }

            return EqualityComparer<TValue?>.Default.Equals(Context.Value, Value);
        }
    }

    /// <summary>
    /// Gets whether this radio item is disabled (individual or group disabled).
    /// </summary>
    private bool IsDisabled => Disabled || (Context?.Disabled ?? false);

    /// <summary>
    /// Gets the data-state attribute value for CSS hooks.
    /// </summary>
    private string DataState => IsChecked ? "checked" : "unchecked";

    /// <summary>
    /// Handles the radio item click event.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    private async Task HandleClick(MouseEventArgs args)
    {
        if (!IsDisabled && Context?.SelectValue != null && Value != null)
        {
            await Context.SelectValue(Value);
        }
    }

    /// <summary>
    /// Handles keyboard events for accessibility.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <remarks>
    /// Supports Space and Enter keys to select the radio item.
    /// Prevents default behavior to avoid page scrolling.
    /// </remarks>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (!IsDisabled && Context?.SelectValue != null && Value != null &&
            (args.Key == " " || args.Key == "Enter"))
        {
            shouldPreventDefault = true;
            await Context.SelectValue(Value);
        }
        else
        {
            shouldPreventDefault = false;
        }
    }

    /// <summary>
    /// Focuses this radio item programmatically.
    /// </summary>
    public async Task FocusAsync()
    {
        try
        {
            await buttonElement.FocusAsync();
        }
        catch
        {
            // Ignore focus errors (element might not be in DOM yet)
        }
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Auto-generate ID if not provided (important for accessibility and label association)
        if (string.IsNullOrEmpty(Id))
        {
            Id = $"radio-{Guid.NewGuid():N}";
        }

        // Register with parent RadioGroup
        Context?.Items.Add(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // Unregister from parent RadioGroup
        Context?.Items.Remove(this);
    }
}
