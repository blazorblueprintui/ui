using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbTextarea"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldTextarea provides a higher-level abstraction over <see cref="BbTextarea"/>.
/// It automatically handles label association, helper text display, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbTextarea"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbFormFieldTextarea @bind-Value="description"
///                     Label="Description"
///                     HelperText="Max 500 characters"
///                     MaxLength="500"
///                     ShowCharacterCount="true" /&gt;
/// </code>
/// </example>
public partial class BbFormFieldTextarea : FormFieldBase
{
    // --- Textarea Pass-Through Parameters ---

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
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute. Passed through to the inner Textarea.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

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
    /// Gets or sets the maximum number of characters allowed.
    /// </summary>
    [Parameter]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets whether to display a character count below the textarea.
    /// </summary>
    [Parameter]
    public bool ShowCharacterCount { get; set; }

    /// <summary>
    /// Gets or sets when ValueChanged fires.
    /// </summary>
    [Parameter]
    public UpdateTiming UpdateTiming { get; set; } = UpdateTiming.OnChange;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Textarea element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(string? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
