using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a column in a DataView with declarative syntax.
/// Columns drive sorting and filtering behavior in the parent DataView.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <typeparam name="TValue">The type of the column's value.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewColumn provides a declarative way to define which properties of a data
/// item participate in sorting and filtering. Each column specifies how to extract
/// data (Property), a display label (Header), and optional sort/filter flags.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewColumn TItem="Person" TValue="string"
///                   Property="@(p => p.Name)"
///                   Header="Name"
///                   Sortable="true"
///                   Filterable="true" /&gt;
/// </code>
/// </example>
public partial class BbDataViewColumn<TItem, TValue> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the unique identifier for this column.
    /// If not provided, it will be auto-generated from the Header.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the display label for this column used in sort selectors.
    /// </summary>
    [Parameter, EditorRequired]
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function that extracts the column value from a data item.
    /// Required for sorting and filtering to work on this column.
    /// </summary>
    [Parameter]
    public Func<TItem, TValue?>? Property { get; set; }

    /// <summary>
    /// Gets or sets whether items can be sorted by this column.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Gets or sets whether this column participates in global search filtering.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Gets or sets the parent DataView component.
    /// Automatically set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataView<TItem>? ParentView { get; set; }

    /// <summary>
    /// Gets the effective column ID (uses Id if provided, otherwise generates from Header).
    /// </summary>
    internal string EffectiveId => Id ?? Header.ToLowerInvariant().Replace(" ", "-");

    protected override void OnInitialized()
    {
        if (ParentView == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataViewColumn<TItem, TValue>)} must be placed inside a {nameof(BbDataView<TItem>)} component.");
        }

        ParentView.RegisterField(this);
    }
}
