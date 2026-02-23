using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the item rendering template for a DataView.
/// Place this component inside a BbDataView to specify how each item is rendered.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewTemplate captures a typed render fragment that BbDataView uses to render
/// each item. The context variable provides the current data item.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewTemplate TItem="Person" Context="person"&gt;
///     &lt;div class="p-4 border rounded-lg"&gt;
///         &lt;h3&gt;@person.Name&lt;/h3&gt;
///         &lt;p&gt;@person.Email&lt;/p&gt;
///     &lt;/div&gt;
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

        ParentView.SetItemTemplate(ChildContent);
    }
}
