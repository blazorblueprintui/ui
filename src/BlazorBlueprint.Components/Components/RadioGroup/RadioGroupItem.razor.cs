using Microsoft.AspNetCore.Components;
using System.Text;
using BlazorBlueprint.Primitives.RadioGroup;

namespace BlazorBlueprint.Components.RadioGroup;

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
/// <code>
/// &lt;RadioGroupItem Value="@("option1")" Id="r1" /&gt;
/// </code>
/// </example>
public partial class RadioGroupItem<TValue> : ComponentBase
{
    private BlazorBlueprint.Primitives.RadioGroup.RadioGroupItem<TValue>? primitiveRef;

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
    private string CssClass
    {
        get
        {
            var builder = new StringBuilder();

            // Base radio button styles (from shadcn/ui)
            builder.Append("aspect-square h-4 w-4 rounded-full border border-primary ");
            builder.Append("text-primary ring-offset-background ");
            builder.Append("focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ");
            builder.Append("disabled:cursor-not-allowed disabled:opacity-50 ");

            // Layout for centering the inner circle
            builder.Append("flex items-center justify-center ");

            // Custom classes (if provided)
            if (!string.IsNullOrWhiteSpace(Class))
            {
                builder.Append(Class);
            }

            return builder.ToString().Trim();
        }
    }

    /// <summary>
    /// Gets the computed CSS classes for the inner circle indicator.
    /// </summary>
    private string CircleIndicatorClass
    {
        get
        {
            var builder = new StringBuilder();

            // Inner circle that appears when selected
            builder.Append("h-2.5 w-2.5 rounded-full bg-current ");

            // Only visible when checked
            if (!IsChecked)
            {
                builder.Append("scale-0 ");
            }

            // Transition for smooth appearance
            builder.Append("transition-transform duration-100");

            return builder.ToString().Trim();
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
