using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components.Checkbox;

/// <summary>
/// A checkbox component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The Checkbox component provides a customizable, accessible checkbox that supports
/// two-way data binding, form validation, disabled states, and indeterminate state.
/// It follows WCAG 2.1 AA standards for accessibility and integrates with Blazor's form system.
/// </para>
/// <para>
/// Features:
/// - Two-way binding support with @bind-Checked
/// - Indeterminate state support (for "select all" scenarios)
/// - Form validation integration (EditContext)
/// - Disabled state support
/// - Full keyboard navigation (Space to toggle)
/// - ARIA attributes for screen readers
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Checkbox @bind-Checked="isAccepted" /&gt;
///
/// &lt;Checkbox Checked="@someChecked"
///           Indeterminate="@someIndeterminate"
///           CheckedChanged="@HandleCheckedChanged" /&gt;
/// </code>
/// </example>
public partial class Checkbox : ComponentBase
{
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is checked.
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
    /// It is invoked whenever the user toggles the checkbox state.
    /// Also notifies EditContext for form validation.
    /// </remarks>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is in an indeterminate state.
    /// </summary>
    /// <remarks>
    /// The indeterminate state is typically used for "select all" checkboxes
    /// when only some child items are selected. When indeterminate is true,
    /// a dash icon is displayed instead of a checkmark.
    /// </remarks>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the indeterminate state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IndeterminateChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled:
    /// - Checkbox cannot be clicked or focused
    /// - Opacity is reduced
    /// - Pointer events are disabled
    /// - aria-disabled attribute is set to true
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the checkbox.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the checkbox.
    /// </summary>
    /// <remarks>
    /// Provides accessible text for screen readers when the checkbox
    /// doesn't have associated label text.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID attribute for the checkbox element.
    /// </summary>
    /// <remarks>
    /// Used for associating the checkbox with label elements via htmlFor attribute.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    /// <remarks>
    /// Used for form validation integration. When provided, the checkbox
    /// registers with the EditContext and participates in form validation.
    /// </remarks>
    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    /// <summary>
    /// Gets whether the checkbox is in an invalid state (for validation).
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
    /// Gets the computed CSS classes for the checkbox element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base checkbox styles (size, border, transitions)
    /// - State-specific classes (checked, indeterminate, disabled)
    /// - Focus ring for keyboard navigation
    /// - Custom classes from the Class parameter
    /// </remarks>
    private string CssClass => ClassNames.cn(
        "peer h-4 w-4 shrink-0 rounded-sm border border-primary",
        "ring-offset-background focus-visible:outline-none focus-visible:ring-2",
        "focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:cursor-not-allowed disabled:opacity-50",
        (Checked || Indeterminate) ? "bg-primary text-primary-foreground" : "bg-background",
        Class
    );

    /// <summary>
    /// Handles the checked state change and notifies EditContext for form validation.
    /// </summary>
    private async Task HandleCheckedChanged(bool value)
    {
        Checked = value;
        await CheckedChanged.InvokeAsync(value);

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
