using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the item rendering template for one layout mode of a DataView.
/// Place this component inside a BbDataView to specify how each item is rendered in
/// list mode, grid mode, or both.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewTemplate captures a typed render fragment that BbDataView uses to render
/// each item in the specified layout mode. Use Layout="DataViewLayout.List" for the list
/// template and Layout="DataViewLayout.Grid" for the grid template. Place two instances
/// (one for each layout) to enable the toolbar layout-toggle buttons.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewTemplate TItem="Person" Layout="DataViewLayout.List" Context="person"&gt;
///     &lt;div class="flex items-center gap-3 p-4"&gt;@person.Name&lt;/div&gt;
/// &lt;/BbDataViewTemplate&gt;
/// &lt;BbDataViewTemplate TItem="Person" Layout="DataViewLayout.Grid" Context="person"&gt;
///     &lt;div class="p-4 border rounded-lg"&gt;@person.Name&lt;/div&gt;
/// &lt;/BbDataViewTemplate&gt;
/// </code>
/// </example>
public partial class BbDataViewTemplate<TItem> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the render fragment used to display each item.
    /// The context parameter provides the data item of type TItem.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the layout mode this template applies to.
    /// Use DataViewLayout.List for the list template and DataViewLayout.Grid for the grid template.
    /// Default is DataViewLayout.List.
    /// </summary>
    [Parameter]
    public DataViewLayout Layout { get; set; } = DataViewLayout.List;

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
                $"{nameof(BbDataViewTemplate<TItem>)} must be placed inside a {nameof(BbDataView<TItem>)} component.");
        }

        if (Layout == DataViewLayout.Grid)
        {
            ParentView.SetGridTemplate(ChildContent);
        }
        else
        {
            ParentView.SetListTemplate(ChildContent);
        }
    }
}
