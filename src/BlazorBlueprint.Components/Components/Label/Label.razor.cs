using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Label;

/// <summary>
/// A label component that renders a semantic HTML label element for form fields.
/// </summary>
/// <remarks>
/// <para>
/// The Label component follows the shadcn/ui design system and provides accessible labeling
/// for form controls. It automatically associates with form fields using the htmlFor attribute
/// and supports disabled state styling through CSS peer selectors.
/// </para>
/// <para>
/// Common use cases include:
/// <list type="bullet">
/// <item>Form field labels with explicit association</item>
/// <item>Required field indicators</item>
/// <item>Accessible checkbox and radio button labels</item>
/// <item>Interactive labels that control focus</item>
/// </list>
/// </para>
/// <para>
/// The component uses Tailwind CSS peer-disabled utilities to automatically style the label
/// when its associated form control is disabled, providing visual feedback without additional code.
/// </para>
/// </remarks>
/// <example>
/// Basic label with htmlFor association:
/// <code>
/// &lt;Label For="email"&gt;Email Address&lt;/Label&gt;
/// &lt;Input Id="email" Type="InputType.Email" /&gt;
/// </code>
///
/// Required field indicator:
/// <code>
/// &lt;Label For="username"&gt;
///     Username &lt;span class="text-destructive"&gt;*&lt;/span&gt;
/// &lt;/Label&gt;
/// &lt;Input Id="username" Required /&gt;
/// </code>
///
/// Label for checkbox:
/// <code>
/// &lt;div class="flex items-center space-x-2"&gt;
///     &lt;Checkbox Id="terms" /&gt;
///     &lt;Label For="terms"&gt;Accept terms and conditions&lt;/Label&gt;
/// &lt;/div&gt;
/// </code>
/// </example>
public partial class Label : ComponentBase
{
    /// <summary>
    /// Gets or sets the ID of the form element this label is associated with.
    /// </summary>
    /// <value>
    /// A string containing the ID of the target form control, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// This parameter maps to the HTML <c>for</c> attribute (htmlFor in JSX).
    /// When set, clicking the label will focus or activate the associated form control.
    /// <para>
    /// Best practices:
    /// <list type="bullet">
    /// <item>Always provide a For value for explicit label-control association</item>
    /// <item>Ensure the For value matches the Id of the target form control</item>
    /// <item>Use meaningful IDs that describe the field's purpose</item>
    /// </list>
    /// </para>
    /// </remarks>
    [Parameter]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the label element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS class names, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Use this parameter to customize the label's appearance beyond default styling.
    /// Common Tailwind utilities include:
    /// <list type="bullet">
    /// <item>Text size: <c>text-lg</c>, <c>text-sm</c></item>
    /// <item>Font weight: <c>font-bold</c>, <c>font-normal</c></item>
    /// <item>Color: <c>text-muted-foreground</c>, <c>text-destructive</c></item>
    /// <item>Spacing: <c>mb-2</c>, <c>mr-2</c></item>
    /// </list>
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the label element.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the label's content, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Typically contains the label text, but can include additional elements such as:
    /// <list type="bullet">
    /// <item>Required field indicators (asterisks, badges)</item>
    /// <item>Help text or tooltips</item>
    /// <item>Icons or visual indicators</item>
    /// <item>Nested spans for styling portions of text</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS class string for the label element.
    /// </summary>
    /// <value>
    /// A string containing all CSS classes to be applied to the label element.
    /// </value>
    /// <remarks>
    /// The class string includes:
    /// <list type="bullet">
    /// <item>Base text styles: <c>text-sm font-medium leading-none</c></item>
    /// <item>Disabled state: <c>peer-disabled:cursor-not-allowed peer-disabled:opacity-70</c></item>
    /// <item>Custom classes: Any classes provided via the <see cref="Class"/> parameter</item>
    /// </list>
    /// <para>
    /// The peer-disabled utilities work when the label follows a form control with the
    /// <c>peer</c> class. When that control is disabled, the label automatically receives
    /// reduced opacity and cursor-not-allowed styling.
    /// </para>
    /// </remarks>
    private string CssClass => ClassNames.cn(
        "text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70",
        Class
    );
}
