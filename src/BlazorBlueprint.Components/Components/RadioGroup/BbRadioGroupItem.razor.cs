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
        "bb:aspect-square bb:h-4 bb:w-4 bb:rounded-full bb:border-2",
        IsChecked ? "bb:border-primary bb:bg-primary" : "bb:border-input",
        "bb:text-primary bb:ring-offset-background",
        "bb:focus:outline-none bb:focus-visible:ring-2 bb:focus-visible:ring-ring bb:focus-visible:ring-offset-2",
        "bb:disabled:cursor-not-allowed bb:disabled:opacity-50",
        "bb:flex bb:items-center bb:justify-center",
        HasLabel ? "bb:peer" : null,
        Class
    );

    /// <summary>
    /// Gets the computed CSS classes for the inner circle indicator.
    /// </summary>
    private string CircleIndicatorClass => ClassNames.cn(
        "bb:h-2 bb:w-2 bb:rounded-full bb:bg-background",
        !IsChecked ? "bb:scale-0" : null,
        "bb:transition-transform bb:duration-100"
    );

    /// <summary>
    /// Gets the computed CSS classes for the label element.
    /// </summary>
    private static string LabelCssClass => "bb:text-sm bb:font-medium bb:leading-none bb:peer-disabled:cursor-not-allowed bb:peer-disabled:opacity-70";

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
