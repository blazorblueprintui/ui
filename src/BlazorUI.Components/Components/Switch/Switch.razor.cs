using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace BlazorUI.Components.Switch;

/// <summary>
/// Size variants for the Switch component.
/// </summary>
public enum SwitchSize
{
    /// <summary>
    /// Small size switch (h-5 w-9, thumb h-4 w-4).
    /// </summary>
    Small,

    /// <summary>
    /// Medium size switch (default) (h-6 w-11, thumb h-5 w-5).
    /// </summary>
    Medium,

    /// <summary>
    /// Large size switch (h-7 w-14, thumb h-6 w-6).
    /// </summary>
    Large
}

/// <summary>
/// A switch component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The Switch component provides a customizable, accessible toggle control that supports
/// two-way data binding, form validation, and disabled states. It follows WCAG 2.1 AA
/// standards for accessibility and integrates with Blazor's event and form systems.
/// </para>
/// <para>
/// Features:
/// - Two-way binding support with @bind-Checked
/// - Form validation integration (EditContext)
/// - Multiple size variants (Small, Medium, Large)
/// - Disabled state support
/// - Full keyboard navigation (Space/Enter to toggle)
/// - ARIA attributes for screen readers
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Switch @bind-Checked="isEnabled" Size="SwitchSize.Medium"&gt;
/// &lt;/Switch&gt;
/// </code>
/// </example>
public partial class Switch : ComponentBase
{
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets whether the switch is checked (on).
    /// </summary>
    /// <remarks>
    /// This property supports two-way binding using the @bind-Checked directive.
    /// Changes to this property trigger the CheckedChanged event callback.
    /// </remarks>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the checked state changes.
    /// </summary>
    /// <remarks>
    /// This event callback enables two-way binding with @bind-Checked.
    /// It is invoked whenever the user toggles the switch state.
    /// </remarks>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the switch is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled:
    /// - Switch cannot be clicked or focused
    /// - Opacity is reduced
    /// - Pointer events are disabled
    /// - aria-disabled attribute is set to true
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the size variant of the switch.
    /// </summary>
    /// <remarks>
    /// Available sizes:
    /// - Small: Compact switch for dense layouts
    /// - Medium: Default size (recommended)
    /// - Large: Prominent switch for primary actions
    /// </remarks>
    [Parameter]
    public SwitchSize Size { get; set; } = SwitchSize.Medium;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the switch.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the switch.
    /// </summary>
    /// <remarks>
    /// Provides accessible text for screen readers when the switch
    /// doesn't have associated label text.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID attribute for the switch element.
    /// </summary>
    /// <remarks>
    /// Used for associating the switch with label elements via htmlFor attribute.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    /// <remarks>
    /// Used for form validation integration. When provided, the switch
    /// registers with the EditContext and participates in form validation.
    /// </remarks>
    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    /// <summary>
    /// Gets whether the switch is in an invalid state (for validation).
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            if (_editContext != null && CheckedExpression != null && _fieldIdentifier.FieldName != null)
            {
                return _editContext.GetValidationMessages(_fieldIdentifier).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Gets the computed CSS classes for the switch track element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base switch track styles (rounded, transitions)
    /// - Size-specific dimensions
    /// - State-specific classes (checked, disabled)
    /// - Focus ring for keyboard navigation
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base switch styles (from shadcn/ui)
        "peer inline-flex shrink-0 cursor-pointer items-center rounded-full border-2 border-transparent",
        "transition-colors focus-visible:outline-none focus-visible:ring-2",
        "focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background",
        "disabled:cursor-not-allowed disabled:opacity-50",
        // Size variants
        Size switch
        {
            SwitchSize.Small => "h-5 w-9",
            SwitchSize.Medium => "h-6 w-11",
            SwitchSize.Large => "h-7 w-14",
            _ => "h-6 w-11"
        },
        // Checked state styling
        Checked ? "bg-primary" : "bg-input",
        // Custom classes (if provided)
        Class
    );

    /// <summary>
    /// Gets the computed CSS classes for the switch thumb element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base thumb styles (rounded circle, background)
    /// - Size-specific dimensions
    /// - Translation based on checked state
    /// - Smooth transitions
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string ThumbCssClass
    {
        get
        {
            // Size variants and translations
            var (thumbSize, translateX) = Size switch
            {
                SwitchSize.Small => ("h-4 w-4", Checked ? "translate-x-4" : "translate-x-0"),
                SwitchSize.Medium => ("h-5 w-5", Checked ? "translate-x-5" : "translate-x-0"),
                SwitchSize.Large => ("h-6 w-6", Checked ? "translate-x-7" : "translate-x-0"),
                _ => ("h-5 w-5", Checked ? "translate-x-5" : "translate-x-0")
            };

            return ClassNames.cn(
                // Base thumb styles
                "pointer-events-none block rounded-full bg-background shadow-lg ring-0 transition-transform",
                // Size and translation
                thumbSize,
                translateX
            );
        }
    }

    /// <summary>
    /// Handles the checked state change from the primitive and wraps form validation.
    /// </summary>
    private async Task HandleCheckedChanged(bool value)
    {
        Checked = value;
        await CheckedChanged.InvokeAsync(Checked);

        // Notify EditContext of field change for validation
        if (_editContext != null && CheckedExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Initialize EditContext integration if available
        if (CascadedEditContext != null && CheckedExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(CheckedExpression);
        }
    }
}
