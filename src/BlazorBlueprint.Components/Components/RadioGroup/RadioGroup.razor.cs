using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components.RadioGroup;

/// <summary>
/// A radio group component that allows single selection from a set of mutually exclusive options.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with radio group items.</typeparam>
/// <remarks>
/// <para>
/// The RadioGroup component provides an accessible container for radio buttons following
/// the shadcn/ui design system. It supports two-way data binding, form validation, and
/// full keyboard navigation.
/// </para>
/// <para>
/// Features:
/// - Generic TValue support for type-safe binding
/// - Two-way binding support with @bind-Value
/// - Form validation integration (EditContext)
/// - Disabled state support
/// - Full keyboard navigation (Arrow keys, Space/Enter)
/// - ARIA attributes for screen readers
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;RadioGroup @bind-Value="selectedOption"&gt;
///     &lt;div class="flex items-center gap-3"&gt;
///         &lt;RadioGroupItem Value="@("option1")" Id="r1" /&gt;
///         &lt;label for="r1"&gt;Option 1&lt;/label&gt;
///     &lt;/div&gt;
///     &lt;div class="flex items-center gap-3"&gt;
///         &lt;RadioGroupItem Value="@("option2")" Id="r2" /&gt;
///         &lt;label for="r2"&gt;Option 2&lt;/label&gt;
///     &lt;/div&gt;
/// &lt;/RadioGroup&gt;
/// </code>
/// </example>
public partial class RadioGroup<TValue> : ComponentBase
{
    private FieldIdentifier fieldIdentifier;
    private EditContext? editContext;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    /// <remarks>
    /// This property supports two-way binding using the @bind-Value directive.
    /// Changes to this property trigger the ValueChanged event callback.
    /// </remarks>
    [Parameter]
    public TValue Value { get; set; } = default!;

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
    /// Gets or sets additional CSS classes to apply to the radio group container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

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
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    /// <remarks>
    /// Used for form validation integration. When provided, the radio group
    /// registers with the EditContext and participates in form validation.
    /// </remarks>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets whether the radio group is in an invalid state (for validation).
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            if (editContext != null && ValueExpression != null && fieldIdentifier.FieldName != null)
            {
                return editContext.GetValidationMessages(fieldIdentifier).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Gets the computed CSS classes for the radio group container.
    /// </summary>
    private string CssClass => ClassNames.cn("grid gap-2", Class);

    /// <summary>
    /// Handles the value change and notifies EditContext for form validation.
    /// </summary>
    private async Task HandleValueChanged(TValue value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);

        // Notify EditContext of field change for validation
        if (editContext != null && ValueExpression != null && fieldIdentifier.FieldName != null)
        {
            editContext.NotifyFieldChanged(fieldIdentifier);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Initialize EditContext integration if available
        if (CascadedEditContext != null && ValueExpression != null)
        {
            editContext = CascadedEditContext;
            fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }
}
