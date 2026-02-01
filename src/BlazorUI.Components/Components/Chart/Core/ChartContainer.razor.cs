using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// A styled container wrapper for chart components.
/// </summary>
/// <remarks>
/// <para>
/// ChartContainer provides consistent styling and structure for charts including:
/// - Rounded borders and padding matching shadcn/ui Card component
/// - Dark mode compatible background
/// - Flex container for proper chart layout
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ChartContainer&gt;
///     &lt;BarChart Items="@data" ... /&gt;
/// &lt;/ChartContainer&gt;
/// </code>
/// </example>
public partial class ChartContainer : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to display inside the container.
    /// </summary>
    /// <remarks>
    /// Typically contains a chart component (BarChart, LineChart, etc.).
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the container.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex flex-col rounded-lg border bg-card text-card-foreground shadow-sm p-6",
        Class
    );
}
