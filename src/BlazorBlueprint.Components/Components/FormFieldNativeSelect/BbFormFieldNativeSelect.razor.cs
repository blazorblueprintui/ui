using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbNativeSelect{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldNativeSelect provides a higher-level abstraction over <see cref="BbNativeSelect{TValue}"/>.
/// It automatically handles label association, helper text display, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbNativeSelect{TValue}"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
/// <example>
/// <code>
/// &lt;BbFormFieldNativeSelect TValue="string"
///                          @bind-Value="country"
///                          Label="Country"
///                          Placeholder="Select a country"
///                          HelperText="Choose your country of residence"&gt;
///     &lt;option value="us"&gt;United States&lt;/option&gt;
///     &lt;option value="uk"&gt;United Kingdom&lt;/option&gt;
/// &lt;/BbFormFieldNativeSelect&gt;
/// </code>
/// </example>
public partial class BbFormFieldNativeSelect<TValue> : FormFieldBase
{
    // --- NativeSelect Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown when no value is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the select is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the size variant of the select.
    /// </summary>
    [Parameter]
    public NativeSelectSize Size { get; set; } = NativeSelectSize.Default;

    /// <summary>
    /// Gets or sets the content to be rendered inside the select element.
    /// Should contain <c>&lt;option&gt;</c> elements.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner NativeSelect element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
