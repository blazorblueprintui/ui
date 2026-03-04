using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Components;

/// <summary>
/// Abstract base class for form field wrappers that compose a control with
/// <see cref="BbField.BbField"/>, <see cref="BbFieldLabel"/>, <see cref="BbFieldDescription"/>,
/// and <see cref="BbFieldError"/> for a complete, accessible form field experience.
/// </summary>
/// <remarks>
/// Provides shared logic for ID generation, EditContext lifecycle management,
/// ARIA attribute wiring, and error state computation. Subclasses supply the
/// specific control and its field expression.
/// </remarks>
public abstract class FormFieldBase : ComponentBase, IDisposable
{
    private FieldIdentifier? _fieldIdentifier;
    private LambdaExpression? _cachedExpression;
    private EditContext? _subscribedEditContext;

    private readonly string _controlId = $"formfield-{Guid.NewGuid():N}";

    /// <summary>
    /// The auto-generated ID for the inner control element.
    /// </summary>
    protected string ControlId => _controlId;

    /// <summary>
    /// The auto-generated ID for the description element.
    /// </summary>
    protected string DescriptionId => $"{ControlId}-description";

    /// <summary>
    /// The auto-generated ID for the error element.
    /// </summary>
    protected string ErrorId => $"{ControlId}-error";

    // --- Shared Parameters ---

    /// <summary>
    /// Gets or sets the label text displayed above or beside the control.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the helper text displayed below the control.
    /// </summary>
    /// <remarks>
    /// Hidden when the field is in an error state; the error message takes its place.
    /// </remarks>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>
    /// Gets or sets a manual error text override displayed when the field has validation errors.
    /// </summary>
    /// <remarks>
    /// This is a display-only override: it does <b>not</b> trigger an error state by itself.
    /// When the field is invalid (e.g., has EditContext validation errors) and this property is set,
    /// the provided text is shown instead of the auto-generated validation messages.
    /// </remarks>
    [Parameter]
    public string? ErrorText { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the field layout.
    /// </summary>
    [Parameter]
    public FieldOrientation Orientation { get; set; } = FieldOrientation.Vertical;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the outer Field container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the control.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    // --- Computed Properties ---

    /// <summary>
    /// Gets whether the field currently has errors from the EditContext.
    /// </summary>
    protected bool HasEditContextErrors => EditContextErrors.Any();

    /// <summary>
    /// Gets the validation error messages from the EditContext.
    /// </summary>
    protected IEnumerable<string> EditContextErrors
    {
        get
        {
            if (CascadedEditContext is not null && _fieldIdentifier.HasValue)
            {
                return CascadedEditContext.GetValidationMessages(_fieldIdentifier.Value);
            }

            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Gets whether the field is in an invalid state. Subclasses may override
    /// to add additional error sources (e.g., parse errors).
    /// </summary>
    protected virtual bool IsInvalid => HasEditContextErrors;

    /// <summary>
    /// Gets the value for the <c>aria-describedby</c> attribute. Points to
    /// the error element when invalid, or the description element when helper text
    /// is present. Subclasses may override for custom logic.
    /// </summary>
    protected virtual string? DescribedById => IsInvalid
        ? ErrorId
        : !string.IsNullOrEmpty(HelperText) ? DescriptionId : null;

    // --- Abstract Members ---

    /// <summary>
    /// Returns the lambda expression for the bound field, used to create
    /// a <see cref="FieldIdentifier"/> for EditContext integration.
    /// </summary>
    protected abstract LambdaExpression? GetFieldExpression();

    /// <summary>
    /// Notifies the cascaded <see cref="EditContext"/> that the field value has changed,
    /// triggering validation. Call this after updating the bound value.
    /// </summary>
    protected void NotifyFieldChanged()
    {
        if (CascadedEditContext is not null && _fieldIdentifier.HasValue)
        {
            CascadedEditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }
    }

    // --- Lifecycle ---

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CascadedEditContext != _subscribedEditContext)
        {
            if (_subscribedEditContext is not null)
            {
                _subscribedEditContext.OnValidationStateChanged -= OnValidationStateChanged;
            }

            _subscribedEditContext = CascadedEditContext;

            if (_subscribedEditContext is not null)
            {
                _subscribedEditContext.OnValidationStateChanged += OnValidationStateChanged;
            }
        }

        var expression = GetFieldExpression();
        if (CascadedEditContext is not null && expression is not null)
        {
            // Only recompute when the expression reference changes to avoid
            // Expression.Compile() on every render cycle.
            if (!ReferenceEquals(expression, _cachedExpression))
            {
                _cachedExpression = expression;
                _fieldIdentifier = CreateFieldIdentifier(expression);
            }
        }
        else
        {
            _cachedExpression = null;
            _fieldIdentifier = null;
        }
    }

    private static FieldIdentifier CreateFieldIdentifier(LambdaExpression expression)
    {
        // FieldIdentifier.Create<T> requires a typed Expression<Func<T>>, but we receive
        // a LambdaExpression. Parse the member expression to extract model + field name.
        if (expression.Body is MemberExpression memberExpression)
        {
            var model = EvaluateExpression(memberExpression.Expression!);
            return new FieldIdentifier(model, memberExpression.Member.Name);
        }

        // Fallback for unary conversions (e.g. boxing of value types)
        if (expression.Body is UnaryExpression { Operand: MemberExpression innerMember })
        {
            var model = EvaluateExpression(innerMember.Expression!);
            return new FieldIdentifier(model, innerMember.Member.Name);
        }

        throw new ArgumentException("The provided expression must be a member access expression.", nameof(expression));
    }

    private static object EvaluateExpression(Expression expression) =>
        Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile()();

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) =>
        StateHasChanged();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_subscribedEditContext is not null)
        {
            _subscribedEditContext.OnValidationStateChanged -= OnValidationStateChanged;
        }

        GC.SuppressFinalize(this);
    }
}
