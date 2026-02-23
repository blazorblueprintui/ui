using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the footer slot for a DataView.
/// Place this component inside a BbDataView to provide custom footer content
/// rendered below the items and pagination.
/// </summary>
/// <remarks>
/// <para>
/// BbDataViewFooter registers its child content with the parent BbDataView, which
/// renders it at the bottom of the view with appropriate styling.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewFooter&gt;
///     &lt;p class="text-sm text-muted-foreground"&gt;Showing filtered results&lt;/p&gt;
/// &lt;/BbDataViewFooter&gt;
/// </code>
/// </example>
public partial class BbDataViewFooter : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to display in the footer slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the parent DataView interface.
    /// Automatically set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    private IDataViewParent? ParentView { get; set; }

    protected override void OnInitialized() => ParentView?.SetFooterContent(ChildContent);
}
