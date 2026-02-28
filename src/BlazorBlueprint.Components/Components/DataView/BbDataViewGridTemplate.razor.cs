using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the item rendering template for grid layout mode of a DataView.
/// Place this component inside a BbDataView to specify how each item is rendered in
/// grid mode. Use alongside BbDataViewListTemplate to enable the toolbar layout-toggle.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewGridTemplate captures a typed render fragment that BbDataView uses to render
/// each item in grid layout. When only a BbDataViewGridTemplate is placed (without a
/// BbDataViewListTemplate) the view locks into grid mode and hides the layout-toggle buttons.
/// Provide both BbDataViewListTemplate and BbDataViewGridTemplate to enable the toggle.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataView TItem="Product" Data="@products"&gt;
///     &lt;BbDataViewGridTemplate TItem="Product" Context="product"&gt;
///         &lt;div class="p-4 border rounded-lg"&gt;@product.Name&lt;/div&gt;
///     &lt;/BbDataViewGridTemplate&gt;
/// &lt;/BbDataView&gt;
/// </code>
/// </example>
public partial class BbDataViewGridTemplate<TItem> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the render fragment used to display each item in grid layout.
    /// The context parameter provides the data item of type TItem.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the parent DataView component.
    /// Automatically set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataView<TItem>? ParentView { get; set; }

    protected override void OnInitialized()
    {
        if (ParentView == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataViewGridTemplate<TItem>)} must be placed inside a {nameof(BbDataView<TItem>)} component.");
        }

        ParentView.SetGridTemplate(ChildContent);
    }
}
