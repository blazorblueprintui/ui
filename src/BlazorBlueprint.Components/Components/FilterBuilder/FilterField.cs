namespace BlazorBlueprint.Components;

/// <summary>
/// Defines metadata for a field available in the filter builder.
/// </summary>
public class FilterField
{
    /// <summary>
    /// Gets the field name, used to identify the field and match against data properties.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the display label shown in the field selector dropdown.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets the data type of the field, which determines available operators and value input.
    /// </summary>
    public FilterFieldType Type { get; init; }

    /// <summary>
    /// Gets the predefined options for <see cref="FilterFieldType.Enum"/> fields.
    /// Each option maps a string value to a display label.
    /// </summary>
    public IEnumerable<SelectOption<string>>? Options { get; init; }

    /// <summary>
    /// Gets the placeholder text shown in the value input.
    /// </summary>
    public string? Placeholder { get; init; }
}
