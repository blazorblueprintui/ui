using Microsoft.AspNetCore.Components;
using BlazorBlueprint.Primitives.RadioGroup;

namespace BlazorBlueprint.Components;

/// <summary>
/// A radio button item that can be selected within a RadioGroup.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with this radio item.</typeparam>
/// <remarks>
/// <para>
/// The RadioGroupItem component represents a single selectable option within a RadioGroup.
/// It displays as a circle with an inner dot when selected, following the shadcn/ui design.
/// </para>
/// <para>
/// Features:
/// - Circle with inner dot visual styling
/// - Selected state management via parent RadioGroup
/// - Disabled state support
/// - Keyboard navigation (Space/Enter to select)
/// - ARIA attributes for accessibility
/// - Focus management for keyboard navigation
/// </para>
/// </remarks>
/// <example>
/// With Label parameter (recommended):
/// <code>
/// &lt;RadioGroupItem Value="@("option1")" Label="Option 1" /&gt;
/// </code>
///
/// Without Label (manual label wiring):
/// <code>
/// &lt;RadioGroupItem Value="@("option1")" Id="r1" /&gt;
/// </code>
/// </example>
public partial class BbRadioGroupItem<TValue> : ComponentBase
{
    private BlazorBlueprint.Primitives.RadioGroup.BbRadioGroupItem<TValue>? primitiveRef;

    /// <summary>
    /// Gets or sets the cascaded RadioGroup context from the parent.
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
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether this individual radio item is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled, the item cannot be selected and appears with reduced opacity.
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the radio item.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

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
    /// Gets or sets the ID attribute for the radio item element.
    /// </summary>
    /// <remarks>
    /// Used for associating the radio item with label elements via htmlFor attribute.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets an optional label text to display next to the radio item.
    /// </summary>
    /// <remarks>
    /// When provided, the component renders a wrapper div containing the radio button
    /// and a label element with automatic for/id wiring. Clicking the label activates
    /// the radio button. The label respects disabled state styling via Tailwind peer utilities.
    /// When null, the component renders only the radio button (backwards compatible).
    /// </remarks>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets whether this radio item has a label to render.
    /// </summary>
    private bool HasLabel => !string.IsNullOrEmpty(Label);

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
    /// Gets the computed CSS classes for the radio item button.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "aspect-square h-4 w-4 rounded-full border-2",
        IsChecked ? "border-primary bg-primary" : "border-input",
        "text-primary ring-offset-background",
        "focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "flex items-center justify-center",
        HasLabel ? "peer" : null,
        Class
    );

    /// <summary>
    /// Gets the computed CSS classes for the inner circle indicator.
    /// </summary>
    private string CircleIndicatorClass => ClassNames.cn(
        "h-2 w-2 rounded-full bg-background",
        !IsChecked ? "scale-0" : null,
        "transition-transform duration-100"
    );

    /// <summary>
    /// Gets the computed CSS classes for the label element.
    /// </summary>
    private static string LabelCssClass => "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (HasLabel && string.IsNullOrEmpty(Id))
        {
            Id = $"radio-{Guid.NewGuid():N}";
        }
    }

    /// <summary>
    /// Focuses this radio item programmatically.
    /// </summary>
    internal async Task FocusAsync()
    {
        if (primitiveRef != null)
        {
            await primitiveRef.FocusAsync();
        }
    }
}
