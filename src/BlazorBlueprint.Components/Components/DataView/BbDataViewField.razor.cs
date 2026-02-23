using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a field in a DataView with declarative syntax.
/// Fields drive sorting and filtering behavior in the parent DataView.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <typeparam name="TValue">The type of the field's value.</typeparam>
/// <remarks>
/// <para>
/// BbDataViewField provides a declarative way to define which properties of a data
/// item participate in sorting and filtering. Each field specifies how to extract
/// data (Property), a display label (Header), and optional sort/filter flags.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataViewField TItem="Person" TValue="string"
///                  Property="@(p => p.Name)"
///                  Header="Name"
///                  Sortable="true"
///                  Filterable="true" /&gt;
/// </code>
/// </example>
public partial class BbDataViewField<TItem, TValue> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the unique identifier for this field.
    /// If not provided, it will be auto-generated from the Header.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the display label for this field used in sort selectors.
    /// </summary>
    [Parameter, EditorRequired]
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function that extracts the field value from a data item.
    /// Required for sorting and filtering to work on this field.
    /// </summary>
    [Parameter]
    public Func<TItem, TValue?>? Property { get; set; }

    /// <summary>
    /// Gets or sets whether this field can be used for sorting.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Gets or sets whether this field participates in global search filtering.
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
    /// Gets the effective field ID (uses Id if provided, otherwise generates from Header).
    /// </summary>
    internal string EffectiveId => Id ?? Header.ToLowerInvariant().Replace(" ", "-");

    protected override void OnInitialized()
    {
        if (ParentView == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataViewField<TItem, TValue>)} must be placed inside a {nameof(BbDataView<TItem>)} component.");
        }

        ParentView.RegisterField(this);
    }
}
