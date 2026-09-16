using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A container component that bridges fill definitions to their parent series.
/// </summary>
/// <remarks>
/// <para>
/// Fill acts as a mediator between a series component (Line, Bar, Area, etc.) and
/// a fill definition (e.g., <see cref="BbLinearGradient"/>). When a fill component
/// registers with Fill, it applies the fill color/gradient to the parent series.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Area DataKey="desktop"&gt;
///     &lt;Fill&gt;
///         &lt;LinearGradient&gt;
///             &lt;GradientStop Offset="0" Color="var(--chart-1)" Opacity="0.8" /&gt;
///             &lt;GradientStop Offset="1" Color="var(--chart-1)" Opacity="0.1" /&gt;
///         &lt;/LinearGradient&gt;
///     &lt;/Fill&gt;
/// &lt;/Area&gt;
/// </code>
/// </example>
public partial class BbFill : ComponentBase
{
    [CascadingParameter]
    private SeriesBase? ParentSeries { get; set; }

    /// <summary>
    /// Gets or sets the child content containing fill definition components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private IFillComponent? fillComponent;

    /// <summary>
    /// Called by a child fill definition component to register itself.
    /// </summary>
    /// <param name="component">The fill component providing the fill definition.</param>
    internal void RegisterFill(IFillComponent component)
    {
        fillComponent = component;

        // Set the provider for lazy evaluation — BuildFill is called during
        // BuildSeriesCore (in OnAfterRenderAsync) when all child GradientStops exist.
        ParentSeries?.FillProvider = component;
    }
}
