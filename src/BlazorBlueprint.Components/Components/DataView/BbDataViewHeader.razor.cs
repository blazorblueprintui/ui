using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the header slot for a DataView.
/// Place this component inside a BbDataView to provide custom header content
/// rendered above the toolbar and items.
/// </summary>
/// <remarks>
/// <para>
/// BbDataViewHeader registers its child content with the parent BbDataView, which
/// renders it at the top of the view with appropriate styling.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewHeader&gt;
///     &lt;div class="flex items-center justify-between"&gt;
///         &lt;h2 class="text-xl font-semibold"&gt;My Data&lt;/h2&gt;
///     &lt;/div&gt;
/// &lt;/BbDataViewHeader&gt;
/// </code>
/// </example>
public partial class BbDataViewHeader : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to display in the header slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the parent DataView interface.
    /// Automatically set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    private IDataViewParent? ParentView { get; set; }

    protected override void OnInitialized() => ParentView?.SetHeaderContent(ChildContent);
}
