using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Collapsible;

/// <summary>
/// A headless primitive component that provides collapsible functionality with expandable/collapsible content sections.
/// </summary>
/// <remarks>
/// <para>
/// The Collapsible component is a headless (unstyled) component that handles all the behavior,
/// state management, and accessibility features for a collapsible/disclosure pattern. It provides no
/// default styling, allowing complete control over appearance.
/// </para>
/// <para>
/// This is a composite component pattern consisting of:
/// <list type="bullet">
/// <item><see cref="BbCollapsible"/> - Container that manages state and provides context</item>
/// <item><see cref="BbCollapsibleTrigger"/> - Button that toggles the open/closed state</item>
/// <item><see cref="BbCollapsibleContent"/> - Content area that expands/collapses</item>
/// </list>
/// </para>
/// <para>
/// The component supports controlled and uncontrolled modes:
/// <list type="bullet">
/// <item>Controlled: Parent manages state via Open parameter and OpenChanged callback</item>
/// <item>Uncontrolled: Component manages its own internal state</item>
/// </list>
/// </para>
/// <para>
/// Included accessibility features:
/// <list type="bullet">
/// <item>ARIA attributes (aria-expanded, aria-controls)</item>
/// <item>Keyboard support (Space, Enter)</item>
/// <item>Focus management</item>
/// <item>Semantic HTML structure</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic collapsible (uncontrolled):
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         Toggle
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         This content can be expanded or collapsed.
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
///
/// Controlled collapsible:
/// <code>
/// &lt;Collapsible @bind-Open="isOpen"&gt;
///     &lt;CollapsibleTrigger&gt;
///         @(isOpen ? "Collapse" : "Expand")
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         Controlled content visibility.
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
///
/// @@code {
///     private bool isOpen = false;
/// }
/// </code>
/// </example>
public partial class BbCollapsible : ComponentBase
{
    private CollapsibleContext context = new();

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is currently expanded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is open (content visible); otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Supports two-way binding via @bind-Open syntax.
    /// </remarks>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the open state changes.
    /// </summary>
    /// <value>
    /// An <see cref="EventCallback{Boolean}"/> that receives the new open state.
    /// </value>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is disabled and cannot be toggled;
    /// otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered inside the collapsible container.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the child components.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the container element.
    /// </summary>
    /// <value>
    /// A dictionary of additional HTML attributes.
    /// </value>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Initializes the component on first render.
    /// </summary>
    protected override void OnInitialized()
    {
        context.Open = Open;
        context.Disabled = Disabled;
        context.Toggle = ToggleAsync;
    }

    /// <summary>
    /// Updates the context when parameters change.
    /// </summary>
    protected override void OnParametersSet()
    {
        context.Open = Open;
        context.Disabled = Disabled;
    }

    /// <summary>
    /// Toggles the open/closed state of the collapsible.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task ToggleAsync()
    {
        if (Disabled)
        {
            return;
        }

        Open = !Open;
        context.Open = Open;

        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(Open);
        }

        StateHasChanged();
    }
}
