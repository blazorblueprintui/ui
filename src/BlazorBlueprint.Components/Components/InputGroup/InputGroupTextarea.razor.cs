using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components.InputGroup;

/// <summary>
/// A textarea component optimized for use within InputGroup.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupTextarea is a specialized textarea designed to work seamlessly within
/// an InputGroup container. It removes standalone styling since the parent provides
/// the visual container, border, and focus management.
/// </para>
/// <para>
/// Features:
/// - Transparent background for seamless integration
/// - No border or focus ring (parent handles these)
/// - Flexible height based on content
/// - Resize control options
/// - Automatic marking for parent detection
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;InputGroup&gt;
///     &lt;InputGroupTextarea Rows="4" Placeholder="Write a comment..." /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.BlockEnd"&gt;
///         &lt;InputGroupText&gt;0 / 280&lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class InputGroupTextarea : ComponentBase
{
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the current value of the textarea.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the textarea value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the number of visible text rows.
    /// </summary>
    /// <remarks>
    /// Default is 3 rows. The textarea can grow beyond this if resize is enabled.
    /// </remarks>
    [Parameter]
    public int Rows { get; set; } = 3;

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ARIA described-by attribute.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea value is invalid.
    /// </summary>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute for the textarea element.
    /// </summary>
    /// <remarks>
    /// When inside an EditForm and not explicitly set, the name is automatically
    /// derived from the ValueExpression (FieldIdentifier) to support SSR form postback.
    /// </remarks>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    private bool IsInvalid
    {
        get
        {
            if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
            {
                return _editContext.GetValidationMessages(_fieldIdentifier).Any();
            }
            return false;
        }
    }

    private string? EffectiveAriaInvalid
    {
        get
        {
            if (AriaInvalid == true || IsInvalid)
            {
                return "true";
            }
            return AriaInvalid?.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => Name ?? (_editContext != null && _fieldIdentifier.FieldName != null ? _fieldIdentifier.FieldName : null);

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the textarea element.
    /// </summary>
    /// <remarks>
    /// Uses minimal styling since the parent InputGroup provides the visual container.
    /// Prevents resize by default for better control over layout.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base styles - minimal for group context
        "flex-1 bg-transparent px-3 py-2 text-base min-h-[60px]",
        "border-0 rounded-none", // No border or radius for seamless integration
        "placeholder:text-muted-foreground",
        "focus-visible:outline-none",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "resize-none", // Prevent resize for cleaner appearance
        // Medium screens and up: smaller text
        "md:text-sm",
        // Custom classes
        Class
    );

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CascadedEditContext != null && ValueExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    /// <summary>
    /// Handles the input event (fired on every keystroke).
    /// </summary>
    private async Task HandleInput(ChangeEventArgs args)
    {
        var newValue = args.Value?.ToString();
        if (string.IsNullOrWhiteSpace(newValue))
        {
            newValue = null;
        }

        Value = newValue;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(newValue);
        }

        if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    /// <summary>
    /// Handles the change event (fired when textarea loses focus).
    /// </summary>
    private static async Task HandleChange(ChangeEventArgs args) =>
        await Task.CompletedTask;
}
