using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the item rendering template for list layout mode of a DataView.
/// Place this component inside a BbDataView to specify how each item is rendered in
/// list mode. Use alongside BbDataViewGridTemplate to enable the toolbar layout-toggle.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewListTemplate captures a typed render fragment that BbDataView uses to render
/// each item in list layout. When only a BbDataViewListTemplate is placed (without a
/// BbDataViewGridTemplate) the view locks into list mode and hides the layout-toggle buttons.
/// Provide both BbDataViewListTemplate and BbDataViewGridTemplate to enable the toggle.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataView TItem="Person" Data="@people"&gt;
///     &lt;BbDataViewListTemplate TItem="Person" Context="person"&gt;
///         &lt;div class="flex items-center gap-3 p-4"&gt;@person.Name&lt;/div&gt;
///     &lt;/BbDataViewListTemplate&gt;
/// &lt;/BbDataView&gt;
/// </code>
/// </example>
public partial class BbDataViewListTemplate<TItem> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the render fragment used to display each item in list layout.
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
                $"{nameof(BbDataViewListTemplate<TItem>)} must be placed inside a {nameof(BbDataView<TItem>)} component.");
        }

        ParentView.SetListTemplate(ChildContent);
    }
}
