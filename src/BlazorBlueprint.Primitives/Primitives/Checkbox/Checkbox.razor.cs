using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Primitives.Checkbox;

/// <summary>
/// A headless checkbox primitive that provides behavior and accessibility without styling.
/// </summary>
/// <remarks>
/// <para>
/// The CheckboxPrimitive component is a headless primitive that handles all checkbox behavior,
/// keyboard navigation, and accessibility features. It provides no default styling.
/// </para>
/// <para>
/// This primitive supports three states:
/// <list type="bullet">
/// <item>Checked (true)</item>
/// <item>Unchecked (false)</item>
/// <item>Indeterminate (mixed) - typically used for "select all" scenarios</item>
/// </list>
/// </para>
/// <para>
/// Accessibility features (WCAG 2.1 AA):
/// <list type="bullet">
/// <item>Semantic button element with checkbox role</item>
/// <item>aria-checked attribute (true/false/mixed for indeterminate)</item>
/// <item>Keyboard support (Space key to toggle)</item>
/// <item>Disabled state with appropriate ARIA attributes</item>
/// <item>data-state attribute for CSS styling hooks</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic headless checkbox:
/// <code>
/// &lt;CheckboxPrimitive @bind-Checked="isAccepted" /&gt;
/// </code>
///
/// Styled checkbox with custom classes:
/// <code>
/// &lt;CheckboxPrimitive @bind-Checked="isChecked"
///                    class="h-4 w-4 rounded border border-primary"&gt;
///     @if (isChecked)
///     {
///         &lt;svg&gt;...checkmark icon...&lt;/svg&gt;
///     }
/// &lt;/CheckboxPrimitive&gt;
/// </code>
///
/// Indeterminate checkbox:
/// <code>
/// &lt;CheckboxPrimitive Checked="@someChecked"
///                    Indeterminate="@someIndeterminate"
///                    CheckedChanged="@HandleCheckedChanged" /&gt;
/// </code>
/// </example>
public partial class Checkbox : ComponentBase
{
    private bool shouldPreventDefault;

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
    /// </remarks>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is in an indeterminate state.
    /// </summary>
    /// <remarks>
    /// The indeterminate state is typically used for "select all" checkboxes
    /// when only some child items are selected. When indeterminate is true,
    /// the aria-checked attribute is set to "mixed" for screen readers.
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
    /// - aria-disabled attribute is set to true
    /// - Keyboard events are ignored
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the ID attribute for the checkbox element.
    /// </summary>
    /// <remarks>
    /// Used for associating the checkbox with label elements via htmlFor attribute.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

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
    /// Gets or sets the content to be rendered inside the checkbox.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the checkbox content (e.g., checkmark icon), or <c>null</c>.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the button element.
    /// </summary>
    /// <value>
    /// A dictionary of additional HTML attributes including class, style, etc.
    /// </value>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the aria-checked attribute value.
    /// </summary>
    /// <remarks>
    /// Returns "mixed" for indeterminate, "true" for checked, "false" for unchecked.
    /// </remarks>
    private string AriaCheckedValue
    {
        get
        {
            if (Indeterminate)
            {
                return "mixed";
            }

            return Checked.ToString().ToLower(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Gets the data-state attribute value for CSS hooks.
    /// </summary>
    private string DataState
    {
        get
        {
            if (Indeterminate)
            {
                return "indeterminate";
            }

            return Checked ? "checked" : "unchecked";
        }
    }

    /// <summary>
    /// Handles the checkbox click event.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <remarks>
    /// Toggles the checked state and clears indeterminate state.
    /// Does nothing if the checkbox is disabled.
    /// </remarks>
    private async Task HandleClick(MouseEventArgs args)
    {
        if (!Disabled)
        {
            await ToggleChecked();
        }
    }

    /// <summary>
    /// Handles keyboard events for accessibility.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <remarks>
    /// Supports Space key to toggle the checkbox state.
    /// Prevents default behavior to avoid page scrolling.
    /// </remarks>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (!Disabled && args.Key == " ")
        {
            shouldPreventDefault = true;
            await ToggleChecked();
        }
        else
        {
            shouldPreventDefault = false;
        }
    }

    /// <summary>
    /// Toggles the checked state and notifies listeners.
    /// </summary>
    /// <remarks>
    /// Clears the indeterminate state when toggling.
    /// </remarks>
    private async Task ToggleChecked()
    {
        // Clear indeterminate state when user clicks
        if (Indeterminate)
        {
            Indeterminate = false;
            await IndeterminateChanged.InvokeAsync(false);
        }

        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);
    }
}
