namespace BlazorBlueprint.Components;

/// <summary>
/// Internal interface for columns that support per-column filtering.
/// Provides metadata needed to build the filter UI and construct filter expressions.
/// </summary>
internal interface IFilterableColumn
{
    /// <summary>
    /// Gets the filter field type for this column's data type.
    /// </summary>
    internal FilterFieldType GetFilterFieldType();

    /// <summary>
    /// Gets the predefined options for Enum filter fields, or null for other types.
    /// </summary>
    internal IEnumerable<SelectOption<string>>? GetFilterOptions();

    /// <summary>
    /// Gets the field name used in the <see cref="FilterCondition.Field"/> property.
    /// Typically the property name from the expression.
    /// </summary>
    internal string GetFilterFieldName();
}
